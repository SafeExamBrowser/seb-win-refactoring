/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.WinForms.Handler;
using CefSharp.WinForms.Host;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Applications.Contracts.Events;
using SafeExamBrowser.Browser.Contracts.Events;
using SafeExamBrowser.Browser.Contracts.Filters;
using SafeExamBrowser.Browser.Events;
using SafeExamBrowser.Browser.Filters;
using SafeExamBrowser.Browser.Handlers;
using SafeExamBrowser.Browser.Wrapper;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Browser;
using SafeExamBrowser.Settings.Browser.Filter;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Browser;
using SafeExamBrowser.UserInterface.Contracts.Browser.Data;
using SafeExamBrowser.UserInterface.Contracts.FileSystemDialog;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using Syroot.Windows.IO;
using BrowserSettings = SafeExamBrowser.Settings.Browser.BrowserSettings;
using DisplayHandler = SafeExamBrowser.Browser.Handlers.DisplayHandler;
using Request = SafeExamBrowser.Browser.Contracts.Filters.Request;
using ResourceHandler = SafeExamBrowser.Browser.Handlers.ResourceHandler;
using TitleChangedEventHandler = SafeExamBrowser.Applications.Contracts.Events.TitleChangedEventHandler;

namespace SafeExamBrowser.Browser
{
	internal class BrowserWindow : IApplicationWindow
	{
		private const string CLEAR_FIND_TERM = "thisisahacktoclearthesearchresultsasitappearsthatthereisnosuchfunctionalityincef";
		private const double ZOOM_FACTOR = 0.2;

		private readonly AppConfig appConfig;
		private readonly IFileSystemDialog fileSystemDialog;
		private readonly IHashAlgorithm hashAlgorithm;
		private readonly HttpClient httpClient;
		private readonly bool isMainWindow;
		private readonly IKeyGenerator keyGenerator;
		private readonly IModuleLogger logger;
		private readonly IMessageBox messageBox;
		private readonly SessionMode sessionMode;
		private readonly Dictionary<int, BrowserWindow> popups;
		private readonly BrowserSettings settings;
		private readonly string startUrl;
		private readonly IText text;
		private readonly IUserInterfaceFactory uiFactory;

		private (string term, bool isInitial, bool caseSensitive, bool forward) findParameters;
		private IBrowserWindow window;
		private double zoomLevel;

		private WindowSettings WindowSettings
		{
			get { return isMainWindow ? settings.MainWindow : settings.AdditionalWindow; }
		}

		internal IBrowserControl Control { get; private set; }
		internal int Id { get; }

		public IntPtr Handle { get; private set; }
		public IconResource Icon { get; private set; }
		public string Title { get; private set; }

		internal event WindowClosedEventHandler Closed;
		internal event DownloadRequestedEventHandler ConfigurationDownloadRequested;
		internal event PopupRequestedEventHandler PopupRequested;
		internal event ResetRequestedEventHandler ResetRequested;
		internal event SessionIdentifierDetectedEventHandler SessionIdentifierDetected;
		internal event LoseFocusRequestedEventHandler LoseFocusRequested;
		internal event TerminationRequestedEventHandler TerminationRequested;

		public event IconChangedEventHandler IconChanged;
		public event TitleChangedEventHandler TitleChanged;

		public BrowserWindow(
			AppConfig appConfig,
			IFileSystemDialog fileSystemDialog,
			IHashAlgorithm hashAlgorithm,
			int id,
			bool isMainWindow,
			IKeyGenerator keyGenerator,
			IModuleLogger logger,
			IMessageBox messageBox,
			SessionMode sessionMode,
			BrowserSettings settings,
			string startUrl,
			IText text,
			IUserInterfaceFactory uiFactory)
		{
			this.appConfig = appConfig;
			this.fileSystemDialog = fileSystemDialog;
			this.hashAlgorithm = hashAlgorithm;
			this.httpClient = new HttpClient();
			this.Id = id;
			this.isMainWindow = isMainWindow;
			this.keyGenerator = keyGenerator;
			this.logger = logger;
			this.messageBox = messageBox;
			this.popups = new Dictionary<int, BrowserWindow>();
			this.sessionMode = sessionMode;
			this.settings = settings;
			this.startUrl = startUrl;
			this.text = text;
			this.uiFactory = uiFactory;
		}

		public void Activate()
		{
			window.BringToForeground();
		}

		internal void Close()
		{
			window.Close();
			Control.Destroy();
		}

		internal void Focus(bool forward)
		{
			if (forward)
			{
				window.FocusToolbar(forward);
			}
			else
			{
				window.FocusBrowser();
				Activate();
			}
		}

		internal void InitializeControl()
		{
			var cefSharpControl = default(ICefSharpControl);
			var dialogHandler = new DialogHandler();
			var displayHandler = new DisplayHandler();
			var downloadLogger = logger.CloneFor($"{nameof(DownloadHandler)} #{Id}");
			var downloadHandler = new DownloadHandler(appConfig, downloadLogger, settings, WindowSettings);
			var keyboardHandler = new KeyboardHandler();
			var renderHandler = new RenderProcessMessageHandler(appConfig, keyGenerator, settings, text);
			var requestFilter = new RequestFilter();
			var requestLogger = logger.CloneFor($"{nameof(RequestHandler)} #{Id}");
			var resourceHandler = new ResourceHandler(appConfig, requestFilter, keyGenerator, logger, sessionMode, settings, WindowSettings, text);
			var requestHandler = new RequestHandler(appConfig, requestFilter, requestLogger, resourceHandler, settings, WindowSettings);

			Icon = new BrowserIconResource();

			if (isMainWindow)
			{
				cefSharpControl = new CefSharpBrowserControl(CreateLifeSpanHandlerForMainWindow(), startUrl);
			}
			else
			{
				cefSharpControl = new CefSharpPopupControl();
			}

			dialogHandler.DialogRequested += DialogHandler_DialogRequested;
			displayHandler.FaviconChanged += DisplayHandler_FaviconChanged;
			displayHandler.ProgressChanged += DisplayHandler_ProgressChanged;
			downloadHandler.ConfigurationDownloadRequested += DownloadHandler_ConfigurationDownloadRequested;
			downloadHandler.DownloadAborted += DownloadHandler_DownloadAborted;
			downloadHandler.DownloadUpdated += DownloadHandler_DownloadUpdated;
			keyboardHandler.FindRequested += KeyboardHandler_FindRequested;
			keyboardHandler.FocusAddressBarRequested += KeyboardHandler_FocusAddressBarRequested;
			keyboardHandler.HomeNavigationRequested += HomeNavigationRequested;
			keyboardHandler.ReloadRequested += ReloadRequested;
			keyboardHandler.TabPressed += KeyboardHandler_TabPressed;
			keyboardHandler.ZoomInRequested += ZoomInRequested;
			keyboardHandler.ZoomOutRequested += ZoomOutRequested;
			keyboardHandler.ZoomResetRequested += ZoomResetRequested;
			resourceHandler.SessionIdentifierDetected += (id) => SessionIdentifierDetected?.Invoke(id);
			requestHandler.QuitUrlVisited += RequestHandler_QuitUrlVisited;
			requestHandler.RequestBlocked += RequestHandler_RequestBlocked;

			InitializeRequestFilter(requestFilter);

			Control = new BrowserControl(cefSharpControl, dialogHandler, displayHandler, downloadHandler, keyboardHandler, renderHandler, requestHandler);
			Control.AddressChanged += Control_AddressChanged;
			Control.LoadFailed += Control_LoadFailed;
			Control.LoadingStateChanged += Control_LoadingStateChanged;
			Control.TitleChanged += Control_TitleChanged;

			Control.Initialize();
			logger.Debug("Initialized browser control.");
		}

		internal void InitializeWindow()
		{
			window = uiFactory.CreateBrowserWindow(Control, settings, isMainWindow, this.logger);
			window.AddressChanged += Window_AddressChanged;
			window.BackwardNavigationRequested += Window_BackwardNavigationRequested;
			window.Closed += Window_Closed;
			window.Closing += Window_Closing;
			window.DeveloperConsoleRequested += Window_DeveloperConsoleRequested;
			window.FindRequested += Window_FindRequested;
			window.ForwardNavigationRequested += Window_ForwardNavigationRequested;
			window.HomeNavigationRequested += HomeNavigationRequested;
			window.LoseFocusRequested += Window_LoseFocusRequested;
			window.ReloadRequested += ReloadRequested;
			window.ZoomInRequested += ZoomInRequested;
			window.ZoomOutRequested += ZoomOutRequested;
			window.ZoomResetRequested += ZoomResetRequested;
			window.UpdateZoomLevel(CalculateZoomPercentage());
			window.Show();
			window.BringToForeground();

			Handle = window.Handle;

			logger.Debug("Initialized browser window.");
		}

		private ILifeSpanHandler CreateLifeSpanHandlerForMainWindow()
		{
			return LifeSpanHandler
					.Create(() => LifeSpanHandler_CreatePopup())
					.OnBeforePopupCreated((wb, b, f, u, t, d, g, s) => LifeSpanHandler_PopupRequested(u))
					.OnPopupCreated((c, u) => LifeSpanHandler_PopupCreated(c))
					.OnPopupDestroyed((c, b) => LifeSpanHandler_PopupDestroyed(c))
					.Build();
		}

		private void InitializeRequestFilter(IRequestFilter requestFilter)
		{
			if (settings.Filter.ProcessContentRequests || settings.Filter.ProcessMainRequests)
			{
				var factory = new RuleFactory();

				foreach (var settings in settings.Filter.Rules)
				{
					var rule = factory.CreateRule(settings.Type);

					rule.Initialize(settings);
					requestFilter.Load(rule);
				}

				logger.Debug($"Initialized request filter with {settings.Filter.Rules.Count} rule(s).");

				if (requestFilter.Process(new Request { Url = settings.StartUrl }) != FilterResult.Allow)
				{
					var rule = factory.CreateRule(FilterRuleType.Simplified);

					rule.Initialize(new FilterRuleSettings { Expression = settings.StartUrl, Result = FilterResult.Allow });
					requestFilter.Load(rule);

					logger.Debug($"Automatically created filter rule to allow start URL{(WindowSettings.UrlPolicy.CanLog() ? $" '{settings.StartUrl}'" : "")}.");
				}
			}
		}

		private void Control_AddressChanged(string address)
		{
			logger.Info($"Navigated{(WindowSettings.UrlPolicy.CanLog() ? $" to '{address}'" : "")}.");
			window.UpdateAddress(address);

			if (WindowSettings.UrlPolicy == UrlPolicy.Always || WindowSettings.UrlPolicy == UrlPolicy.BeforeTitle)
			{
				Title = address;
				window.UpdateTitle(address);
				TitleChanged?.Invoke(address);
			}

			AutoFind();
		}

		private void Control_LoadFailed(int errorCode, string errorText, bool isMainRequest, string url)
		{
			switch (errorCode)
			{
				case (int) CefErrorCode.Aborted:
					logger.Info($"Request{(WindowSettings.UrlPolicy.CanLog() ? $" for '{url}'" : "")} was aborted.");
					break;
				case (int) CefErrorCode.InternetDisconnected:
					logger.Info($"Request{(WindowSettings.UrlPolicy.CanLog() ? $" for '{url}'" : "")} has failed due to loss of internet connection.");
					break;
				case (int) CefErrorCode.None:
					logger.Info($"Request{(WindowSettings.UrlPolicy.CanLog() ? $" for '{url}'" : "")} was successful.");
					break;
				case (int) CefErrorCode.UnknownUrlScheme:
					logger.Info($"Request{(WindowSettings.UrlPolicy.CanLog() ? $" for '{url}'" : "")} has an unknown URL scheme and will be handled by the OS.");
					break;
				default:
					HandleUnknownLoadFailure(errorCode, errorText, isMainRequest, url);
					break;
			}
		}

		private void HandleUnknownLoadFailure(int errorCode, string errorText, bool isMainRequest, string url)
		{
			var requestInfo = $"{errorText} ({errorCode}, {(isMainRequest ? "main" : "resource")} request)";

			logger.Warn($"Request{(WindowSettings.UrlPolicy.CanLogError() ? $" for '{url}'" : "")} failed: {requestInfo}.");

			if (isMainRequest)
			{
				var title = text.Get(TextKey.Browser_LoadErrorTitle);
				var message = text.Get(TextKey.Browser_LoadErrorMessage).Replace("%%URL%%", WindowSettings.UrlPolicy.CanLogError() ? url : "") + $" {requestInfo}";

				Task.Run(() => messageBox.Show(message, title, icon: MessageBoxIcon.Error, parent: window)).ContinueWith(_ => Control.NavigateBackwards());
			}
		}

		private void Control_LoadingStateChanged(bool isLoading)
		{
			window.CanNavigateBackwards = WindowSettings.AllowBackwardNavigation && Control.CanNavigateBackwards;
			window.CanNavigateForwards = WindowSettings.AllowForwardNavigation && Control.CanNavigateForwards;
			window.UpdateLoadingState(isLoading);
		}

		private void Control_TitleChanged(string title)
		{
			if (WindowSettings.UrlPolicy != UrlPolicy.Always)
			{
				Title = title;
				window.UpdateTitle(Title);
				TitleChanged?.Invoke(Title);
			}
		}

		private void DialogHandler_DialogRequested(DialogRequestedEventArgs args)
		{
			var isDownload = args.Operation == FileSystemOperation.Save;
			var isUpload = args.Operation == FileSystemOperation.Open;
			var isAllowed = (isDownload && settings.AllowDownloads) || (isUpload && settings.AllowUploads);
			var initialPath = default(string);

			if (isDownload)
			{
				initialPath = args.InitialPath;
			}
			else if (string.IsNullOrEmpty(settings.DownAndUploadDirectory))
			{
				initialPath = KnownFolders.Downloads.ExpandedPath;
			}
			else
			{
				initialPath = Environment.ExpandEnvironmentVariables(settings.DownAndUploadDirectory);
			}

			if (isAllowed)
			{
				var result = fileSystemDialog.Show(
					args.Element,
					args.Operation,
					initialPath,
					title: args.Title,
					parent: window,
					restrictNavigation: !settings.AllowCustomDownAndUploadLocation,
					showElementPath: settings.ShowFileSystemElementPath);

				if (result.Success)
				{
					args.FullPath = result.FullPath;
					args.Success = result.Success;
					logger.Debug($"User selected path '{result.FullPath}' when asked to {args.Operation}->{args.Element}.");
				}
				else
				{
					logger.Debug($"User aborted file system dialog to {args.Operation}->{args.Element}.");
				}
			}
			else
			{
				logger.Info($"Blocked file system dialog to {args.Operation}->{args.Element}, as {(isDownload ? "downloading" : "uploading")} is not allowed.");
				ShowDownUploadNotAllowedMessage(isDownload);
			}
		}

		private void DisplayHandler_FaviconChanged(string uri)
		{
			Task.Run(() =>
			{
				var request = new HttpRequestMessage(HttpMethod.Head, uri);
				var response = httpClient.SendAsync(request).ContinueWith(task =>
				{
					if (task.IsCompleted && task.Result.IsSuccessStatusCode)
					{
						Icon = new BrowserIconResource(uri);

						IconChanged?.Invoke(Icon);
						window.UpdateIcon(Icon);
					}
				});
			});
		}

		private void DisplayHandler_ProgressChanged(double value)
		{
			window.UpdateProgress(value);
		}

		private void DownloadHandler_ConfigurationDownloadRequested(string fileName, DownloadEventArgs args)
		{
			if (settings.AllowConfigurationDownloads)
			{
				logger.Debug($"Forwarding download request for configuration file '{fileName}'.");
				ConfigurationDownloadRequested?.Invoke(fileName, args);

				if (args.AllowDownload)
				{
					logger.Debug($"Download request for configuration file '{fileName}' was granted.");
				}
				else
				{
					logger.Debug($"Download request for configuration file '{fileName}' was denied.");
					messageBox.Show(TextKey.MessageBox_ReconfigurationDenied, TextKey.MessageBox_ReconfigurationDeniedTitle, parent: window);
				}
			}
			else
			{
				logger.Debug($"Discarded download request for configuration file '{fileName}'.");
			}
		}

		private void DownloadHandler_DownloadAborted()
		{
			ShowDownUploadNotAllowedMessage();
		}

		private void DownloadHandler_DownloadUpdated(DownloadItemState state)
		{
			window.UpdateDownloadState(state);
		}

		private void HomeNavigationRequested()
		{
			if (isMainWindow && (settings.UseStartUrlAsHomeUrl || !string.IsNullOrWhiteSpace(settings.HomeUrl)))
			{
				var navigate = false;
				var url = settings.UseStartUrlAsHomeUrl ? settings.StartUrl : settings.HomeUrl;

				if (settings.HomeNavigationRequiresPassword && !string.IsNullOrWhiteSpace(settings.HomePasswordHash))
				{
					var message = text.Get(TextKey.PasswordDialog_BrowserHomePasswordRequired);
					var title = !string.IsNullOrWhiteSpace(settings.HomeNavigationMessage) ? settings.HomeNavigationMessage : text.Get(TextKey.PasswordDialog_BrowserHomePasswordRequiredTitle);
					var dialog = uiFactory.CreatePasswordDialog(message, title);
					var result = dialog.Show(window);

					if (result.Success)
					{
						var passwordHash = hashAlgorithm.GenerateHashFor(result.Password);

						if (settings.HomePasswordHash.Equals(passwordHash, StringComparison.OrdinalIgnoreCase))
						{
							navigate = true;
						}
						else
						{
							messageBox.Show(TextKey.MessageBox_InvalidHomePassword, TextKey.MessageBox_InvalidHomePasswordTitle, icon: MessageBoxIcon.Warning, parent: window);
						}
					}
				}
				else
				{
					var message = text.Get(TextKey.MessageBox_BrowserHomeQuestion);
					var title = !string.IsNullOrWhiteSpace(settings.HomeNavigationMessage) ? settings.HomeNavigationMessage : text.Get(TextKey.MessageBox_BrowserHomeQuestionTitle);
					var result = messageBox.Show(message, title, MessageBoxAction.YesNo, MessageBoxIcon.Question, window);

					navigate = result == MessageBoxResult.Yes;
				}

				if (navigate)
				{
					Control.NavigateTo(url);
				}
			}
		}

		private void KeyboardHandler_FindRequested()
		{
			if (settings.AllowFind)
			{
				window.ShowFindbar();
			}
		}

		private void KeyboardHandler_FocusAddressBarRequested()
		{
			window.FocusAddressBar();
		}

		private void KeyboardHandler_TabPressed(bool shiftPressed)
		{
			Control.ExecuteJavascript("document.activeElement.tagName", result =>
			{
				if (result.Result is string tagName && tagName?.ToUpper() == "BODY")
				{
					// This means the user is now at the start of the focus / tabIndex chain in the website.
					if (shiftPressed)
					{
						window.FocusToolbar(!shiftPressed);
					}
					else
					{
						LoseFocusRequested?.Invoke(true);
					}
				}
			});
		}

		private ChromiumHostControl LifeSpanHandler_CreatePopup()
		{
			var args = new PopupRequestedEventArgs();

			PopupRequested?.Invoke(args);

			var control = args.Window.Control.EmbeddableControl as ChromiumHostControl;
			var id = control.GetHashCode();
			var window = args.Window;

			popups[id] = window;
			window.Closed += (_) => popups.Remove(id);

			return control;
		}

		private void LifeSpanHandler_PopupCreated(ChromiumHostControl control)
		{
			var id = control.GetHashCode();
			var window = popups[id];

			window.InitializeWindow();
		}

		private void LifeSpanHandler_PopupDestroyed(ChromiumHostControl control)
		{
			var id = control.GetHashCode();
			var window = popups[id];

			window.Close();
		}

		private PopupCreation LifeSpanHandler_PopupRequested(string targetUrl)
		{
			var creation = PopupCreation.Cancel;
			var validCurrentUri = Uri.TryCreate(Control.Address, UriKind.Absolute, out var currentUri);
			var validNewUri = Uri.TryCreate(targetUrl, UriKind.Absolute, out var newUri);
			var sameHost = validCurrentUri && validNewUri && string.Equals(currentUri.Host, newUri.Host, StringComparison.OrdinalIgnoreCase);

			switch (settings.PopupPolicy)
			{
				case PopupPolicy.Allow:
				case PopupPolicy.AllowSameHost when sameHost:
					logger.Debug($"Forwarding request to open new window{(WindowSettings.UrlPolicy.CanLog() ? $" for '{targetUrl}'" : "")}...");
					creation = PopupCreation.Continue;
					break;
				case PopupPolicy.AllowSameWindow:
				case PopupPolicy.AllowSameHostAndWindow when sameHost:
					logger.Info($"Discarding request to open new window and loading{(WindowSettings.UrlPolicy.CanLog() ? $" '{targetUrl}'" : "")} directly...");
					Control.NavigateTo(targetUrl);
					break;
				case PopupPolicy.AllowSameHost when !sameHost:
				case PopupPolicy.AllowSameHostAndWindow when !sameHost:
					logger.Info($"Blocked request to open new window{(WindowSettings.UrlPolicy.CanLog() ? $" for '{targetUrl}'" : "")} as it targets a different host.");
					break;
				default:
					logger.Info($"Blocked request to open new window{(WindowSettings.UrlPolicy.CanLog() ? $" for '{targetUrl}'" : "")}.");
					break;
			}

			return creation;
		}

		private void RequestHandler_QuitUrlVisited(string url)
		{
			Task.Run(() =>
			{
				if (settings.ResetOnQuitUrl)
				{
					logger.Info("Forwarding request to reset browser...");
					ResetRequested?.Invoke();
				}
				else
				{
					if (settings.ConfirmQuitUrl)
					{
						var message = text.Get(TextKey.MessageBox_BrowserQuitUrlConfirmation);
						var title = text.Get(TextKey.MessageBox_BrowserQuitUrlConfirmationTitle);
						var result = messageBox.Show(message, title, MessageBoxAction.YesNo, MessageBoxIcon.Question, window);
						var terminate = result == MessageBoxResult.Yes;

						if (terminate)
						{
							logger.Info($"User confirmed termination via quit URL{(WindowSettings.UrlPolicy.CanLog() ? $" '{url}'" : "")}, forwarding request...");
							TerminationRequested?.Invoke();
						}
						else
						{
							logger.Info($"User aborted termination via quit URL{(WindowSettings.UrlPolicy.CanLog() ? $" '{url}'" : "")}.");
						}
					}
					else
					{
						logger.Info($"Automatically requesting termination due to quit URL{(WindowSettings.UrlPolicy.CanLog() ? $" '{url}'" : "")}...");
						TerminationRequested?.Invoke();
					}
				}
			});
		}

		private void RequestHandler_RequestBlocked(string url)
		{
			Task.Run(() =>
			{
				var message = text.Get(TextKey.MessageBox_BrowserNavigationBlocked).Replace("%%URL%%", WindowSettings.UrlPolicy.CanLogError() ? url : "");
				var title = text.Get(TextKey.MessageBox_BrowserNavigationBlockedTitle);

				Control.TitleChanged -= Control_TitleChanged;

				if (url.Equals(startUrl, StringComparison.OrdinalIgnoreCase))
				{
					window.UpdateTitle($"*** {title} ***");
					TitleChanged?.Invoke($"*** {title} ***");
				}

				messageBox.Show(message, title, parent: window);
				Control.TitleChanged += Control_TitleChanged;
			});
		}

		private void ReloadRequested()
		{
			if (WindowSettings.AllowReloading && WindowSettings.ShowReloadWarning)
			{
				var result = messageBox.Show(TextKey.MessageBox_ReloadConfirmation, TextKey.MessageBox_ReloadConfirmationTitle, MessageBoxAction.YesNo, MessageBoxIcon.Question, window);

				if (result == MessageBoxResult.Yes)
				{
					logger.Debug("The user confirmed reloading the current page...");
					Control.Reload();
				}
				else
				{
					logger.Debug("The user aborted reloading the current page.");
				}
			}
			else if (WindowSettings.AllowReloading)
			{
				logger.Debug("Reloading current page...");
				Control.Reload();
			}
			else
			{
				logger.Debug("Blocked reload attempt, as the user is not allowed to reload web pages.");
			}
		}

		private void ShowDownUploadNotAllowedMessage(bool isDownload = true)
		{
			var message = isDownload ? TextKey.MessageBox_DownloadNotAllowed : TextKey.MessageBox_UploadNotAllowed;
			var title = isDownload ? TextKey.MessageBox_DownloadNotAllowedTitle : TextKey.MessageBox_UploadNotAllowedTitle;

			messageBox.Show(message, title, icon: MessageBoxIcon.Warning, parent: window);
		}

		private void Window_AddressChanged(string address)
		{
			var isValid = Uri.TryCreate(address, UriKind.Absolute, out _) || Uri.TryCreate($"https://{address}", UriKind.Absolute, out _);

			if (isValid)
			{
				logger.Debug($"The user requested to navigate to '{address}', the URI is valid.");
				Control.NavigateTo(address);
			}
			else
			{
				logger.Debug($"The user requested to navigate to '{address}', but the URI is not valid.");
				window.UpdateAddress(string.Empty);
			}
		}

		private void Window_BackwardNavigationRequested()
		{
			logger.Debug("Navigating backwards...");
			Control.NavigateBackwards();
		}

		private void Window_Closing()
		{
			logger.Debug($"Window is closing...");
		}

		private void Window_Closed()
		{
			logger.Debug($"Window has been closed.");
			Control.Destroy();
			Closed?.Invoke(Id);
		}

		private void Window_DeveloperConsoleRequested()
		{
			logger.Debug("Showing developer console...");
			Control.ShowDeveloperConsole();
		}

		private void Window_FindRequested(string term, bool isInitial, bool caseSensitive, bool forward = true)
		{
			if (settings.AllowFind)
			{
				findParameters.caseSensitive = caseSensitive;
				findParameters.forward = forward;
				findParameters.isInitial = isInitial;
				findParameters.term = term;

				Control.Find(term, isInitial, caseSensitive, forward);
			}
		}

		private void Window_ForwardNavigationRequested()
		{
			logger.Debug("Navigating forwards...");
			Control.NavigateForwards();
		}

		private void Window_LoseFocusRequested(bool forward)
		{
			LoseFocusRequested?.Invoke(forward);
		}

		private void ZoomInRequested()
		{
			if (settings.AllowPageZoom && CalculateZoomPercentage() < 300)
			{
				zoomLevel += ZOOM_FACTOR;
				Control.Zoom(zoomLevel);
				window.UpdateZoomLevel(CalculateZoomPercentage());
				logger.Debug($"Increased page zoom to {CalculateZoomPercentage()}%.");
			}
		}

		private void ZoomOutRequested()
		{
			if (settings.AllowPageZoom && CalculateZoomPercentage() > 25)
			{
				zoomLevel -= ZOOM_FACTOR;
				Control.Zoom(zoomLevel);
				window.UpdateZoomLevel(CalculateZoomPercentage());
				logger.Debug($"Decreased page zoom to {CalculateZoomPercentage()}%.");
			}
		}

		private void ZoomResetRequested()
		{
			if (settings.AllowPageZoom)
			{
				zoomLevel = 0;
				Control.Zoom(0);
				window.UpdateZoomLevel(CalculateZoomPercentage());
				logger.Debug($"Reset page zoom to {CalculateZoomPercentage()}%.");
			}
		}

		private void AutoFind()
		{
			if (settings.AllowFind && !string.IsNullOrEmpty(findParameters.term) && !CLEAR_FIND_TERM.Equals(findParameters.term, StringComparison.OrdinalIgnoreCase))
			{
				Control.Find(findParameters.term, findParameters.isInitial, findParameters.caseSensitive, findParameters.forward);
			}
		}

		private double CalculateZoomPercentage()
		{
			return (zoomLevel * 25.0) + 100.0;
		}
	}
}
