/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Browser.Handlers;
using SafeExamBrowser.Contracts.Browser;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
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
		private BrowserSettings settings;
		private IText text;
		private IUserInterfaceFactory uiFactory;

		public Guid Id { get; private set; }
		public string Name { get; private set; }
		public IWindow Window { get { return window; } }

		public event DownloadRequestedEventHandler ConfigurationDownloadRequested;
		public event NameChangedEventHandler NameChanged;
		public event TerminatedEventHandler Terminated;

		public BrowserApplicationInstance(
			AppConfig appConfig,
			BrowserSettings settings,
			IText text,
			IUserInterfaceFactory uiFactory,
			bool isMainInstance)
		{
			this.appConfig = appConfig;
			this.isMainInstance = isMainInstance;
			this.settings = settings;
			this.text = text;
			this.uiFactory = uiFactory;
		}

		internal void Initialize()
		{
			var downloadHandler = new DownloadHandler(appConfig, settings);

			Id = Guid.NewGuid();
			downloadHandler.ConfigurationDownloadRequested += (fileName, args) => ConfigurationDownloadRequested?.Invoke(fileName, args);

			control = new BrowserControl(settings, text);
			control.AddressChanged += Control_AddressChanged;
			control.LoadingStateChanged += Control_LoadingStateChanged;
			control.TitleChanged += Control_TitleChanged;
			(control as BrowserControl).DownloadHandler = downloadHandler;
			(control as BrowserControl).Initialize();

			window = uiFactory.CreateBrowserWindow(control, settings);
			window.IsMainWindow = isMainInstance;
			window.Closing += () => Terminated?.Invoke(Id);
			window.AddressChanged += Window_AddressChanged;
			window.ReloadRequested += Window_ReloadRequested;
			window.BackwardNavigationRequested += Window_BackwardNavigationRequested;
			window.ForwardNavigationRequested += Window_ForwardNavigationRequested;
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
			control.NavigateTo(address);
		}

		private void Window_ReloadRequested()
		{
			control.Reload();
		}

		private void Window_BackwardNavigationRequested()
		{
			control.NavigateBackwards();
		}

		private void Window_ForwardNavigationRequested()
		{
			control.NavigateForwards();
		}
	}
}
