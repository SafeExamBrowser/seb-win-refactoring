/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Browser.Events;
using SafeExamBrowser.Browser.Handlers;
using SafeExamBrowser.Contracts.Applications;
using SafeExamBrowser.Contracts.Applications.Events;
using SafeExamBrowser.Contracts.Browser;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.UserInterface.Browser;
using SafeExamBrowser.Contracts.UserInterface.MessageBox;
using SafeExamBrowser.Contracts.UserInterface.Windows;

namespace SafeExamBrowser.Browser
{
	internal class BrowserApplicationInstance : IApplicationInstance
	{
		private AppConfig appConfig;
		private IBrowserControl control;
		private IBrowserWindow window;
		private bool isMainInstance;
		private IMessageBox messageBox;
		private IModuleLogger logger;
		private BrowserSettings settings;
		private IText text;
		private IUserInterfaceFactory uiFactory;
		private string url;

		private BrowserWindowSettings WindowSettings
		{
			get { return isMainInstance ? settings.MainWindowSettings : settings.AdditionalWindowSettings; }
		}

		public InstanceIdentifier Id { get; private set; }
		public string Name { get; private set; }
		public IWindow Window { get { return window; } }

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
			this.isMainInstance = isMainInstance;
			this.messageBox = messageBox;
			this.logger = logger;
			this.settings = settings;
			this.text = text;
			this.uiFactory = uiFactory;
			this.url = url;
		}

		internal void Initialize()
		{
			var contextMenuHandler = new ContextMenuHandler(settings, text);
			var displayHandler = new DisplayHandler();
			var downloadLogger = logger.CloneFor($"{nameof(DownloadHandler)} {Id}");
			var downloadHandler = new DownloadHandler(appConfig, settings, downloadLogger);
			var keyboardHandler = new KeyboardHandler();
			var lifeSpanHandler = new LifeSpanHandler();
			var requestHandler = new RequestHandler(appConfig);

			displayHandler.FaviconChanged += DisplayHandler_FaviconChanged;
			downloadHandler.ConfigurationDownloadRequested += DownloadHandler_ConfigurationDownloadRequested;
			keyboardHandler.ReloadRequested += ReloadRequested;
			keyboardHandler.ZoomInRequested += ZoomInRequested;
			keyboardHandler.ZoomOutRequested += ZoomOutRequested;
			keyboardHandler.ZoomResetRequested += ZoomResetRequested;
			lifeSpanHandler.PopupRequested += LifeSpanHandler_PopupRequested;

			control = new BrowserControl(contextMenuHandler, displayHandler, downloadHandler, keyboardHandler, lifeSpanHandler, requestHandler, url);
			control.AddressChanged += Control_AddressChanged;
			control.LoadingStateChanged += Control_LoadingStateChanged;
			control.TitleChanged += Control_TitleChanged;
			control.Initialize();

			logger.Debug("Initialized browser control.");

			window = uiFactory.CreateBrowserWindow(control, settings, isMainInstance);
			window.Closing += () => Terminated?.Invoke(Id);
			window.AddressChanged += Window_AddressChanged;
			window.ReloadRequested += ReloadRequested;
			window.BackwardNavigationRequested += Window_BackwardNavigationRequested;
			window.ForwardNavigationRequested += Window_ForwardNavigationRequested;
			window.ZoomInRequested += ZoomInRequested;
			window.ZoomOutRequested += ZoomOutRequested;
			window.ZoomResetRequested += ZoomResetRequested;

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
			var icon = new BrowserIconResource(uri);

			IconChanged?.Invoke(icon);
			window.UpdateIcon(icon);
		}

		private void DownloadHandler_ConfigurationDownloadRequested(string fileName, DownloadEventArgs args)
		{
			if (settings.AllowConfigurationDownloads)
			{
				args.BrowserWindow = window;
				logger.Debug($"Forwarding download request for configuration file '{fileName}'.");
				ConfigurationDownloadRequested?.Invoke(fileName, args);
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
			logger.Debug($"The user requested to navigate to '{address}'.");
			control.NavigateTo(address);
		}

		private void Window_BackwardNavigationRequested()
		{
			logger.Debug($"Navigating backwards...");
			control.NavigateBackwards();
		}

		private void Window_ForwardNavigationRequested()
		{
			logger.Debug($"Navigating forwards...");
			control.NavigateForwards();
		}

		private void ZoomInRequested()
		{
			if (settings.AllowPageZoom)
			{
				control.ZoomIn();
				logger.Debug("Increased page zoom.");
			}
		}

		private void ZoomOutRequested()
		{
			if (settings.AllowPageZoom)
			{
				control.ZoomOut();
				logger.Debug("Decreased page zoom.");
			}
		}

		private void ZoomResetRequested()
		{
			if (settings.AllowPageZoom)
			{
				control.ZoomReset();
				logger.Debug("Reset page zoom.");
			}
		}
	}
}
