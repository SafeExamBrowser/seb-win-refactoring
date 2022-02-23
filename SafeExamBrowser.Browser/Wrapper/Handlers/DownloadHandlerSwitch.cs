/*
 * Copyright (c) 2022 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using CefSharp;
using CefSharp.WinForms;
using CefSharp.WinForms.Host;

namespace SafeExamBrowser.Browser.Wrapper.Handlers
{
	internal class DownloadHandlerSwitch : IDownloadHandler
	{
		public void OnBeforeDownload(IWebBrowser webBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
		{
			if (browser.IsPopup)
			{
				var control = ChromiumHostControl.FromBrowser(browser) as CefSharpPopupControl;

				control?.OnBeforeDownload(webBrowser, browser, downloadItem, callback);
			}
			else
			{
				var control = ChromiumWebBrowser.FromBrowser(browser) as CefSharpBrowserControl;

				control?.OnBeforeDownload(webBrowser, browser, downloadItem, callback);
			}
		}

		public void OnDownloadUpdated(IWebBrowser webBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
		{
			if (browser.IsPopup)
			{
				var control = ChromiumHostControl.FromBrowser(browser) as CefSharpPopupControl;

				control?.OnDownloadUpdated(webBrowser, browser, downloadItem, callback);
			}
			else
			{
				var control = ChromiumWebBrowser.FromBrowser(browser) as CefSharpBrowserControl;

				control?.OnDownloadUpdated(webBrowser, browser, downloadItem, callback);
			}
		}
	}
}
