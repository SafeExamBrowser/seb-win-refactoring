/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Browser.Handlers;
using SafeExamBrowser.Contracts.Browser;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Core;
using SafeExamBrowser.Contracts.Core.Events;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.UserInterface.Browser;
using SafeExamBrowser.Contracts.UserInterface.Windows;

namespace SafeExamBrowser.Browser
{
	internal class BrowserApplicationInstance : IApplicationInstance
	{
		private AppConfig appConfig;
		private IBrowserControl control;
		private IBrowserWindow window;
		private bool isMainInstance;
		private IModuleLogger logger;
		private BrowserSettings settings;
		private IText text;
		private IUserInterfaceFactory uiFactory;

		public InstanceIdentifier Id { get; private set; }
		public string Name { get; private set; }
		public IWindow Window { get { return window; } }

		public event DownloadRequestedEventHandler ConfigurationDownloadRequested;
		public event NameChangedEventHandler NameChanged;
		public event InstanceTerminatedEventHandler Terminated;

		public BrowserApplicationInstance(
			AppConfig appConfig,
			BrowserSettings settings,
			InstanceIdentifier id,
			bool isMainInstance,
			IModuleLogger logger,
			IText text,
			IUserInterfaceFactory uiFactory)
		{
			this.appConfig = appConfig;
			this.Id = id;
			this.isMainInstance = isMainInstance;
			this.logger = logger;
			this.settings = settings;
			this.text = text;
			this.uiFactory = uiFactory;
		}

		internal void Initialize()
		{
			var controlLogger = logger.CloneFor($"{nameof(BrowserControl)} {Id}");
			var downloadLogger = logger.CloneFor($"{nameof(DownloadHandler)} {Id}");
			var downloadHandler = new DownloadHandler(appConfig, settings, downloadLogger);

			downloadHandler.ConfigurationDownloadRequested += (fileName, args) => ConfigurationDownloadRequested?.Invoke(fileName, args);

			control = new BrowserControl(appConfig, settings, controlLogger, text);
			control.AddressChanged += Control_AddressChanged;
			control.LoadingStateChanged += Control_LoadingStateChanged;
			control.TitleChanged += Control_TitleChanged;
			(control as BrowserControl).DownloadHandler = downloadHandler;
			(control as BrowserControl).Initialize();

			logger.Debug("Initialized browser control.");

			window = uiFactory.CreateBrowserWindow(control, settings);
			window.IsMainWindow = isMainInstance;
			window.Closing += () => Terminated?.Invoke(Id);
			window.AddressChanged += Window_AddressChanged;
			window.ReloadRequested += Window_ReloadRequested;
			window.BackwardNavigationRequested += Window_BackwardNavigationRequested;
			window.ForwardNavigationRequested += Window_ForwardNavigationRequested;

			logger.Debug("Initialized browser window.");
		}

		private void Control_AddressChanged(string address)
		{
			window.UpdateAddress(address);
		}

		private void Control_LoadingStateChanged(bool isLoading)
		{
			window.UpdateLoadingState(isLoading);
		}

		private void Control_TitleChanged(string title)
		{
			window.UpdateTitle(title);
			NameChanged?.Invoke(title);
		}

		private void Window_AddressChanged(string address)
		{
			logger.Debug($"The user requested to navigate to '{address}'.");
			control.NavigateTo(address);
		}

		private void Window_ReloadRequested()
		{
			logger.Debug($"The user requested to reload the current page.");
			control.Reload();
		}

		private void Window_BackwardNavigationRequested()
		{
			logger.Debug($"The user requested to navigate backwards.");
			control.NavigateBackwards();
		}

		private void Window_ForwardNavigationRequested()
		{
			logger.Debug($"The user requested to navigate forwards.");
			control.NavigateForwards();
		}
	}
}
