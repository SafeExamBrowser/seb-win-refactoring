/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using CefSharp;
using CefSharp.WinForms;
using SafeExamBrowser.Browser.Handlers;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface.Browser;
using SafeExamBrowser.Contracts.UserInterface.Browser.Events;
using BrowserSettings = SafeExamBrowser.Contracts.Configuration.Settings.BrowserSettings;

namespace SafeExamBrowser.Browser
{
	internal class BrowserControl : ChromiumWebBrowser, IBrowserControl
	{
		private AppConfig appConfig;
		private BrowserSettings settings;
		private IDownloadHandler downloadHandler;
		private ILogger logger;
		private IText text;

		private AddressChangedEventHandler addressChanged;
		private LoadingStateChangedEventHandler loadingStateChanged;
		private TitleChangedEventHandler titleChanged;

		event AddressChangedEventHandler IBrowserControl.AddressChanged
		{
			add { addressChanged += value; }
			remove { addressChanged -= value; }
		}

		event LoadingStateChangedEventHandler IBrowserControl.LoadingStateChanged
		{
			add { loadingStateChanged += value; }
			remove { loadingStateChanged -= value; }
		}

		event TitleChangedEventHandler IBrowserControl.TitleChanged
		{
			add { titleChanged += value; }
			remove { titleChanged -= value; }
		}

		public BrowserControl(
			AppConfig appConfig,
			BrowserSettings settings,
			IDownloadHandler downloadHandler,
			ILogger logger,
			IText text) : base(settings.StartUrl)
		{
			this.appConfig = appConfig;
			this.downloadHandler = downloadHandler;
			this.logger = logger;
			this.settings = settings;
			this.text = text;
		}

		public void Initialize()
		{
			AddressChanged += BrowserControl_AddressChanged;
			LoadingStateChanged += (o, args) => loadingStateChanged?.Invoke(args.IsLoading);
			TitleChanged += (o, args) => titleChanged?.Invoke(args.Title);

			DownloadHandler = downloadHandler;
			KeyboardHandler = new KeyboardHandler(settings);
			MenuHandler = new ContextMenuHandler(settings, text);
			RequestHandler = new RequestHandler(appConfig);
		}

		public void NavigateBackwards()
		{
			GetBrowser().GoBack();
		}

		public void NavigateForwards()
		{
			GetBrowser().GoForward();
		}

		public void NavigateTo(string address)
		{
			if (!String.IsNullOrWhiteSpace(address))
			{
				Load(address);
			}
		}

		public void Reload()
		{
			GetBrowser().Reload();
		}

		private void BrowserControl_AddressChanged(object sender, AddressChangedEventArgs args)
		{
			logger.Debug($"Navigated to '{args.Address}'.");
			addressChanged?.Invoke(args.Address);
		}
	}
}
