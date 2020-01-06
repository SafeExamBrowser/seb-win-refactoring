/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using CefSharp;
using SafeExamBrowser.Browser.Events;

namespace SafeExamBrowser.Browser.Handlers
{
	internal class LifeSpanHandler : ILifeSpanHandler
	{
		public event PopupRequestedEventHandler PopupRequested;

		public bool DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
		{
			return false;
		}

		public void OnAfterCreated(IWebBrowser chromiumWebBrowser, IBrowser browser)
		{
		}

		public void OnBeforeClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
		{
		}

		public bool OnBeforePopup(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
		{
			var args = new PopupRequestedEventArgs { Url = targetUrl };

			newBrowser = default(IWebBrowser);
			PopupRequested?.Invoke(args);

			return true;
		}
	}
}
