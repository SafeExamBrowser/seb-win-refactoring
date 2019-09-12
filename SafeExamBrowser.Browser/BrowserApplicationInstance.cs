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
using SafeExamBrowser.Browser.Contracts.Events;
using SafeExamBrowser.Browser.Events;
using SafeExamBrowser.Browser.Handlers;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.Browser;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Browser;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;

namespace SafeExamBrowser.Browser
{
	internal class BrowserApplicationInstance : IApplicationInstance
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

		private BrowserWindowSettings WindowSettings
		{
			get { return isMainInstance ? settings.MainWindow : settings.AdditionalWindow; }
		}

		public InstanceIdentifier Id { get; private set; }
		public string Name { get; private set; }

		public event DownloadRequestedEventHandler ConfigurationDownloadRequested;
		public event IconChangedEventHandler IconChanged;
		public event InstanceTerminatedEventHandler Terminated;
		public event NameChangedEventHandler NameChanged;
		public event PopupRequestedEventHandler PopupRequested;

		public BrowserApplicationInstance(
			AppConfig appConfig,
			BrowserSettings settings,
			InstanceIdentifier id,
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
			window?.BringToForeground();
		}

		internal void Initialize()
		{
			InitializeControl();
			InitializeWindow();
		}

		internal void Terminate()
		{
			window?.Close();
		}

		private void InitializeControl()
		{
			var contextMenuHandler = new ContextMenuHandler();
			var displayHandler = new DisplayHandler();
			var downloadLogger = logger.CloneFor($"{nameof(DownloadHandler)} {Id}");
			var downloadHandler = new DownloadHandler(appConfig, settings, downloadLogger);
			var keyboardHandler = new KeyboardHandler();
			var lifeSpanHandler = new LifeSpanHandler();
			var requestLogger = logger.CloneFor($"{nameof(RequestHandler)} {Id}");
			var requestHandler = new RequestHandler(appConfig, settings.Filter, requestLogger, text);

			displayHandler.FaviconChanged += DisplayHandler_FaviconChanged;
			displayHandler.ProgressChanged += DisplayHandler_ProgressChanged;
			downloadHandler.ConfigurationDownloadRequested += DownloadHandler_ConfigurationDownloadRequested;
			keyboardHandler.ReloadRequested += ReloadRequested;
			keyboardHandler.ZoomInRequested += ZoomInRequested;
			keyboardHandler.ZoomOutRequested += ZoomOutRequested;
			keyboardHandler.ZoomResetRequested += ZoomResetRequested;
			lifeSpanHandler.PopupRequested += LifeSpanHandler_PopupRequested;
			requestHandler.RequestBlocked += RequestHandler_RequestBlocked;

			control = new BrowserControl(contextMenuHandler, displayHandler, downloadHandler, keyboardHandler, lifeSpanHandler, requestHandler, url);
			control.AddressChanged += Control_AddressChanged;
			control.LoadingStateChanged += Control_LoadingStateChanged;
			control.TitleChanged += Control_TitleChanged;

			requestHandler.Initiailize();
			control.Initialize();

			logger.Debug("Initialized browser control.");
		}

		private void InitializeWindow()
		{
			window = uiFactory.CreateBrowserWindow(control, settings, isMainInstance);
			window.Closing += () => Terminated?.Invoke(Id);
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
			window.UpdateTitle(title);
			NameChanged?.Invoke(title);
		}

		private void DisplayHandler_FaviconChanged(string uri)
		{
			var request = new HttpRequestMessage(HttpMethod.Head, uri);
			var response = httpClient.SendAsync(request).ContinueWith(task =>
			{
				if (task.IsCompleted && task.Result.IsSuccessStatusCode)
				{
					var icon = new BrowserIconResource(uri);

					IconChanged?.Invoke(icon);
					window.UpdateIcon(icon);
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
					logger.Debug($"Download request for configuration file '{fileName}' was granted. Asking user to confirm the reconfiguration...");

					var message = TextKey.MessageBox_ReconfigurationQuestion;
					var title = TextKey.MessageBox_ReconfigurationQuestionTitle;
					var result = messageBox.Show(message, title, MessageBoxAction.YesNo, MessageBoxIcon.Question, window);

					args.AllowDownload = result == MessageBoxResult.Yes;
					logger.Info($"The user chose to {(args.AllowDownload ? "start" : "abort")} the reconfiguration.");
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
			if (settings.AllowPopups)
			{
				logger.Debug($"Forwarding request to open new window for '{args.Url}'...");
				PopupRequested?.Invoke(args);
			}
			else
			{
				logger.Debug($"Blocked attempt to open new window for '{args.Url}'.");
			}
		}

		private void RequestHandler_RequestBlocked(string url)
		{
			Task.Run(() =>
			{
				var message = text.Get(TextKey.MessageBox_BrowserNavigationBlocked).Replace("%%URL%%", url);
				var title = text.Get(TextKey.MessageBox_BrowserNavigationBlockedTitle);

				messageBox.Show(message, title, parent: window);
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
