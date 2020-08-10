/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using CefSharp;
using CefSharp.WinForms;
using SafeExamBrowser.UserInterface.Contracts.Browser;
using SafeExamBrowser.UserInterface.Contracts.Browser.Events;

namespace SafeExamBrowser.Browser
{
	internal class BrowserControl : ChromiumWebBrowser, IBrowserControl
	{
		private IContextMenuHandler contextMenuHandler;
		private IDialogHandler dialogHandler;
		private IDisplayHandler displayHandler;
		private IDownloadHandler downloadHandler;
		private IKeyboardHandler keyboardHandler;
		private ILifeSpanHandler lifeSpanHandler;
		private IRequestHandler requestHandler;

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
			IContextMenuHandler contextMenuHandler,
			IDialogHandler dialogHandler,
			IDisplayHandler displayHandler,
			IDownloadHandler downloadHandler,
			IKeyboardHandler keyboardHandler,
			ILifeSpanHandler lifeSpanHandler,
			IRequestHandler requestHandler,
			string url) : base(url)
		{
			this.contextMenuHandler = contextMenuHandler;
			this.dialogHandler = dialogHandler;
			this.displayHandler = displayHandler;
			this.downloadHandler = downloadHandler;
			this.keyboardHandler = keyboardHandler;
			this.lifeSpanHandler = lifeSpanHandler;
			this.requestHandler = requestHandler;
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

		private void BrowserControl_LoadError(object sender, LoadErrorEventArgs e)
		{
			loadFailed?.Invoke((int) e.ErrorCode, e.ErrorText, e.FailedUrl);
		}
	}
}
