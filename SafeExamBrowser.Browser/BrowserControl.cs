/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using CefSharp;
using CefSharp.WinForms;
using SafeExamBrowser.Browser.Content;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Browser;
using SafeExamBrowser.UserInterface.Contracts.Browser.Events;

namespace SafeExamBrowser.Browser
{
	internal class BrowserControl : ChromiumWebBrowser, IBrowserControl
	{
		private readonly AppConfig appConfig;
		private readonly ContentLoader contentLoader;
		private readonly IContextMenuHandler contextMenuHandler;
		private readonly IDialogHandler dialogHandler;
		private readonly IDisplayHandler displayHandler;
		private readonly IDownloadHandler downloadHandler;
		private readonly IKeyGenerator generator;
		private readonly IKeyboardHandler keyboardHandler;
		private readonly ILifeSpanHandler lifeSpanHandler;
		private readonly IRequestHandler requestHandler;
		private readonly IText text;

		private AddressChangedEventHandler addressChanged;
		private LoadFailedEventHandler loadFailed;
		private LoadingStateChangedEventHandler loadingStateChanged;
		private TitleChangedEventHandler titleChanged;

		public bool CanNavigateBackwards => GetBrowser().CanGoBack;
		public bool CanNavigateForwards => GetBrowser().CanGoForward;

		event AddressChangedEventHandler IBrowserControl.AddressChanged
		{
			add { addressChanged += value; }
			remove { addressChanged -= value; }
		}

		event LoadFailedEventHandler IBrowserControl.LoadFailed
		{
			add { loadFailed += value; }
			remove { loadFailed -= value; }
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
			IContextMenuHandler contextMenuHandler,
			IDialogHandler dialogHandler,
			IDisplayHandler displayHandler,
			IDownloadHandler downloadHandler,
			IKeyGenerator generator,
			IKeyboardHandler keyboardHandler,
			ILifeSpanHandler lifeSpanHandler,
			IRequestHandler requestHandler,
			IText text,
			string url) : base(url)
		{
			this.appConfig = appConfig;
			this.contentLoader = new ContentLoader(text);
			this.contextMenuHandler = contextMenuHandler;
			this.dialogHandler = dialogHandler;
			this.displayHandler = displayHandler;
			this.downloadHandler = downloadHandler;
			this.generator = generator;
			this.keyboardHandler = keyboardHandler;
			this.lifeSpanHandler = lifeSpanHandler;
			this.requestHandler = requestHandler;
			this.text = text;
		}

		public void Destroy()
		{
			if (!IsDisposed)
			{
				Dispose(true);
			}
		}

		public void Initialize()
		{
			AddressChanged += (o, args) => addressChanged?.Invoke(args.Address);
			FrameLoadStart += BrowserControl_FrameLoadStart;
			IsBrowserInitializedChanged += BrowserControl_IsBrowserInitializedChanged;
			LoadError += BrowserControl_LoadError;
			LoadingStateChanged += (o, args) => loadingStateChanged?.Invoke(args.IsLoading);
			TitleChanged += (o, args) => titleChanged?.Invoke(args.Title);

			DialogHandler = dialogHandler;
			DisplayHandler = displayHandler;
			DownloadHandler = downloadHandler;
			KeyboardHandler = keyboardHandler;
			LifeSpanHandler = lifeSpanHandler;
			MenuHandler = contextMenuHandler;
			RequestHandler = requestHandler;
		}

		private void BrowserControl_FrameLoadStart(object sender, FrameLoadStartEventArgs e)
		{
			var browserExamKey = generator.CalculateBrowserExamKeyHash(e.Url);
			var configurationKey = generator.CalculateConfigurationKeyHash(e.Url);
			var api = contentLoader.LoadApi(browserExamKey, configurationKey, appConfig.ProgramBuildVersion);

			e.Frame.ExecuteJavaScriptAsync(api);
		}

		public void Find(string term, bool isInitial, bool caseSensitive, bool forward = true)
		{
			this.Find(0, term, forward, caseSensitive, !isInitial);
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
			Load(address);
		}

		public void ShowDeveloperConsole()
		{
			GetBrowser().ShowDevTools();
		}

		public void Reload()
		{
			GetBrowser().Reload();
		}

		public void Zoom(double level)
		{
			GetBrowser().SetZoomLevel(level);
		}

		private void BrowserControl_IsBrowserInitializedChanged(object sender, EventArgs e)
		{
			if (IsBrowserInitialized)
			{
				GetBrowser().GetHost().SetFocus(true);
			}
		}

		private void BrowserControl_LoadError(object sender, LoadErrorEventArgs e)
		{
			loadFailed?.Invoke((int) e.ErrorCode, e.ErrorText, e.FailedUrl);
		}
	}
}
