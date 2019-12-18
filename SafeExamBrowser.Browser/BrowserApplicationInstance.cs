/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Net.Http;
using System.Threading.Tasks;
using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Applications.Contracts.Events;
using SafeExamBrowser.Applications.Contracts.Resources.Icons;
using SafeExamBrowser.Browser.Contracts.Events;
using SafeExamBrowser.Browser.Contracts.Filters;
using SafeExamBrowser.Browser.Events;
using SafeExamBrowser.Browser.Filters;
using SafeExamBrowser.Browser.Handlers;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Browser;
using SafeExamBrowser.Settings.Browser.Filter;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Browser;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;

namespace SafeExamBrowser.Browser
{
	internal class BrowserApplicationInstance : IApplicationWindow
	{
		private const double ZOOM_FACTOR = 0.2;

		private AppConfig appConfig;
		private IBrowserControl control;
		private IBrowserWindow window;
		private HttpClient httpClient;
		private bool isMainInstance;
		private IMessageBox messageBox;
		private IModuleLogger logger;
		private BrowserSettings settings;
		private IText text;
		private IUserInterfaceFactory uiFactory;
		private string url;
		private double zoomLevel;

		private WindowSettings WindowSettings
		{
			get { return isMainInstance ? settings.MainWindow : settings.AdditionalWindow; }
		}

		internal int Id { get; }

		public IntPtr Handle { get; private set; }
		public IconResource Icon { get; private set; }
		public string Title { get; private set; }

		internal event DownloadRequestedEventHandler ConfigurationDownloadRequested;
		internal event PopupRequestedEventHandler PopupRequested;
		internal event InstanceTerminatedEventHandler Terminated;

		public event IconChangedEventHandler IconChanged;
		public event TitleChangedEventHandler TitleChanged;

		public BrowserApplicationInstance(
			AppConfig appConfig,
			BrowserSettings settings,
			int id,
			bool isMainInstance,
			IMessageBox messageBox,
			IModuleLogger logger,
			IText text,
			IUserInterfaceFactory uiFactory,
			string url)
		{
			this.appConfig = appConfig;
			this.Id = id;
			this.httpClient = new HttpClient();
			this.isMainInstance = isMainInstance;
			this.messageBox = messageBox;
			this.logger = logger;
			this.settings = settings;
			this.text = text;
			this.uiFactory = uiFactory;
			this.url = url;
		}

		public void Activate()
		{
			window.BringToForeground();
		}

		internal void Initialize()
		{
			InitializeControl();
			InitializeWindow();
		}

		internal void Terminate()
		{
			window.Close();
		}

		private void InitializeControl()
		{
			var contextMenuHandler = new ContextMenuHandler();
			var displayHandler = new DisplayHandler();
			var downloadLogger = logger.CloneFor($"{nameof(DownloadHandler)} {Id}");
			var downloadHandler = new DownloadHandler(appConfig, settings, downloadLogger);
			var keyboardHandler = new KeyboardHandler();
			var lifeSpanHandler = new LifeSpanHandler();
			var requestFilter = new RequestFilter();
			var requestLogger = logger.CloneFor($"{nameof(RequestHandler)} {Id}");
			var requestHandler = new RequestHandler(appConfig, requestFilter, requestLogger, settings, text);

			Icon = new BrowserIconResource();

			displayHandler.FaviconChanged += DisplayHandler_FaviconChanged;
			displayHandler.ProgressChanged += DisplayHandler_ProgressChanged;
			downloadHandler.ConfigurationDownloadRequested += DownloadHandler_ConfigurationDownloadRequested;
			keyboardHandler.ReloadRequested += ReloadRequested;
			keyboardHandler.ZoomInRequested += ZoomInRequested;
			keyboardHandler.ZoomOutRequested += ZoomOutRequested;
			keyboardHandler.ZoomResetRequested += ZoomResetRequested;
			lifeSpanHandler.PopupRequested += LifeSpanHandler_PopupRequested;
			requestHandler.RequestBlocked += RequestHandler_RequestBlocked;

			InitializeRequestFilter(requestFilter);

			control = new BrowserControl(contextMenuHandler, displayHandler, downloadHandler, keyboardHandler, lifeSpanHandler, requestHandler, url);
			control.AddressChanged += Control_AddressChanged;
			control.LoadingStateChanged += Control_LoadingStateChanged;
			control.TitleChanged += Control_TitleChanged;

			control.Initialize();
			logger.Debug("Initialized browser control.");
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

					logger.Debug($"Automatically created filter rule to allow start URL '{settings.StartUrl}'.");
				}
			}
		}

		private void InitializeWindow()
		{
			window = uiFactory.CreateBrowserWindow(control, settings, isMainInstance);
			window.Closing += Window_Closing;
			window.AddressChanged += Window_AddressChanged;
			window.BackwardNavigationRequested += Window_BackwardNavigationRequested;
			window.DeveloperConsoleRequested += Window_DeveloperConsoleRequested;
			window.ForwardNavigationRequested += Window_ForwardNavigationRequested;
			window.ReloadRequested += ReloadRequested;
			window.ZoomInRequested += ZoomInRequested;
			window.ZoomOutRequested += ZoomOutRequested;
			window.ZoomResetRequested += ZoomResetRequested;
			window.UpdateZoomLevel(CalculateZoomPercentage());
			window.Show();

			Handle = window.Handle;

			logger.Debug("Initialized browser window.");
		}

		private void Control_AddressChanged(string address)
		{
			logger.Debug($"Navigated to '{address}'.");
			window.UpdateAddress(address);
		}

		private void Control_LoadingStateChanged(bool isLoading)
		{
			window.CanNavigateBackwards = WindowSettings.AllowBackwardNavigation && control.CanNavigateBackwards;
			window.CanNavigateForwards = WindowSettings.AllowForwardNavigation && control.CanNavigateForwards;
			window.UpdateLoadingState(isLoading);
		}

		private void Control_TitleChanged(string title)
		{
			Title = title;
			window.UpdateTitle(Title);
			TitleChanged?.Invoke(Title);
		}

		private void DisplayHandler_FaviconChanged(string uri)
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

		private void LifeSpanHandler_PopupRequested(PopupRequestedEventArgs args)
		{
			var validCurrentUri = Uri.TryCreate(control.Address, UriKind.Absolute, out var currentUri);
			var validNewUri = Uri.TryCreate(args.Url, UriKind.Absolute, out var newUri);
			var sameHost = validCurrentUri && validNewUri && string.Equals(currentUri.Host, newUri.Host, StringComparison.OrdinalIgnoreCase);

			switch (settings.PopupPolicy)
			{
				case PopupPolicy.Allow:
				case PopupPolicy.AllowSameHost when sameHost:
					logger.Debug($"Forwarding request to open new window for '{args.Url}'...");
					PopupRequested?.Invoke(args);
					break;
				case PopupPolicy.AllowSameWindow:
				case PopupPolicy.AllowSameHostAndWindow when sameHost:
					logger.Info($"Discarding request to open new window and loading '{args.Url}' directly...");
					control.NavigateTo(args.Url);
					break;
				case PopupPolicy.AllowSameHost when !sameHost:
				case PopupPolicy.AllowSameHostAndWindow when !sameHost:
					logger.Info($"Blocked request to open new window for '{args.Url}' as it targets a different host.");
					break;
				default:
					logger.Info($"Blocked request to open new window for '{args.Url}'.");
					break;
			}
		}

		private void RequestHandler_RequestBlocked(string url)
		{
			Task.Run(() =>
			{
				var message = text.Get(TextKey.MessageBox_BrowserNavigationBlocked).Replace("%%URL%%", url);
				var title = text.Get(TextKey.MessageBox_BrowserNavigationBlockedTitle);

				control.TitleChanged -= Control_TitleChanged;

				if (url == this.url)
				{
					window.UpdateTitle($"*** {title} ***");
					TitleChanged?.Invoke($"*** {title} ***");
				}

				messageBox.Show(message, title, parent: window);
				control.TitleChanged += Control_TitleChanged;
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
					control.Reload();
				}
				else
				{
					logger.Debug("The user aborted reloading the current page.");
				}
			}
			else if (WindowSettings.AllowReloading)
			{
				logger.Debug("Reloading current page...");
				control.Reload();
			}
			else
			{
				logger.Debug("Blocked reload attempt, as the user is not allowed to reload web pages.");
			}
		}

		private void Window_AddressChanged(string address)
		{
			var isValid = Uri.TryCreate(address, UriKind.Absolute, out _) || Uri.TryCreate($"https://{address}", UriKind.Absolute, out _);

			if (isValid)
			{
				logger.Debug($"The user requested to navigate to '{address}', the URI is valid.");
				control.NavigateTo(address);
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
			control.NavigateBackwards();
		}

		private void Window_Closing()
		{
			logger.Info($"Instance has terminated.");
			Terminated?.Invoke(Id);
		}

		private void Window_DeveloperConsoleRequested()
		{
			logger.Debug("Showing developer console...");
			control.ShowDeveloperConsole();
		}

		private void Window_ForwardNavigationRequested()
		{
			logger.Debug("Navigating forwards...");
			control.NavigateForwards();
		}

		private void ZoomInRequested()
		{
			if (settings.AllowPageZoom && CalculateZoomPercentage() < 300)
			{
				zoomLevel += ZOOM_FACTOR;
				control.Zoom(zoomLevel);
				window.UpdateZoomLevel(CalculateZoomPercentage());
				logger.Debug($"Increased page zoom to {CalculateZoomPercentage()}%.");
			}
		}

		private void ZoomOutRequested()
		{
			if (settings.AllowPageZoom && CalculateZoomPercentage() > 25)
			{
				zoomLevel -= ZOOM_FACTOR;
				control.Zoom(zoomLevel);
				window.UpdateZoomLevel(CalculateZoomPercentage());
				logger.Debug($"Decreased page zoom to {CalculateZoomPercentage()}%.");
			}
		}

		private void ZoomResetRequested()
		{
			if (settings.AllowPageZoom)
			{
				zoomLevel = 0;
				control.Zoom(0);
				window.UpdateZoomLevel(CalculateZoomPercentage());
				logger.Debug($"Reset page zoom to {CalculateZoomPercentage()}%.");
			}
		}

		private double CalculateZoomPercentage()
		{
			return (zoomLevel * 25.0) + 100.0;
		}
	}
}
