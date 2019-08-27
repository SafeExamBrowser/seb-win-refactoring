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
using SafeExamBrowser.Contracts.UserInterface.Browser;
using SafeExamBrowser.Contracts.UserInterface.Browser.Events;

namespace SafeExamBrowser.Browser
{
	internal class BrowserControl : ChromiumWebBrowser, IBrowserControl
	{
		private IContextMenuHandler contextMenuHandler;
		private IDisplayHandler displayHandler;
		private IDownloadHandler downloadHandler;
		private IKeyboardHandler keyboardHandler;
		private ILifeSpanHandler lifeSpanHandler;
		private IRequestHandler requestHandler;

		private AddressChangedEventHandler addressChanged;
		private LoadingStateChangedEventHandler loadingStateChanged;
		private TitleChangedEventHandler titleChanged;

		public bool CanNavigateBackwards => GetBrowser().CanGoBack;
		public bool CanNavigateForwards => GetBrowser().CanGoForward;

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
			IContextMenuHandler contextMenuHandler,
			IDisplayHandler displayHandler,
			IDownloadHandler downloadHandler,
			IKeyboardHandler keyboardHandler,
			ILifeSpanHandler lifeSpanHandler,
			IRequestHandler requestHandler,
			string url) : base(url)
		{
			this.contextMenuHandler = contextMenuHandler;
			this.displayHandler = displayHandler;
			this.downloadHandler = downloadHandler;
			this.keyboardHandler = keyboardHandler;
			this.lifeSpanHandler = lifeSpanHandler;
			this.requestHandler = requestHandler;
		}

		public void Initialize()
		{
			AddressChanged += (o, args) => addressChanged?.Invoke(args.Address);
			LoadingStateChanged += (o, args) => loadingStateChanged?.Invoke(args.IsLoading);
			TitleChanged += (o, args) => titleChanged?.Invoke(args.Title);

			DisplayHandler = displayHandler;
			DownloadHandler = downloadHandler;
			KeyboardHandler = keyboardHandler;
			LifeSpanHandler = lifeSpanHandler;
			MenuHandler = contextMenuHandler;
			RequestHandler = requestHandler;
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

		protected override IWindowInfo CreateBrowserWindowInfo(IntPtr handle)
		{
			const uint WS_EX_NOACTIVATE = 0x8000000;
			var windowInfo = base.CreateBrowserWindowInfo(handle);

			// Ensures that input elements within the browser control actually receive input when activated via touch.
			windowInfo.ExStyle &= ~WS_EX_NOACTIVATE;

			return windowInfo;
		}
	}
}
