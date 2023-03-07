/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using CefSharp;
using CefSharp.WinForms;
using CefSharp.WinForms.Handler;
using CefSharp.WinForms.Host;

namespace SafeExamBrowser.Browser.Wrapper.Handlers
{
	internal class DisplayHandlerSwitch : DisplayHandler
	{
		protected override void OnFaviconUrlChange(IWebBrowser chromiumWebBrowser, IBrowser browser, IList<string> urls)
		{
			if (browser.IsPopup)
			{
				var control = ChromiumHostControl.FromBrowser(browser) as CefSharpPopupControl;

				control?.OnFaviconUrlChange(chromiumWebBrowser, browser, urls);
			}
			else
			{
				var control = ChromiumWebBrowser.FromBrowser(browser) as CefSharpBrowserControl;

				control?.OnFaviconUrlChange(chromiumWebBrowser, browser, urls);
			}

			base.OnFaviconUrlChange(chromiumWebBrowser, browser, urls);
		}

		protected override void OnLoadingProgressChange(IWebBrowser chromiumWebBrowser, IBrowser browser, double progress)
		{
			if (browser.IsPopup)
			{
				var control = ChromiumHostControl.FromBrowser(browser) as CefSharpPopupControl;

				control?.OnLoadingProgressChange(chromiumWebBrowser, browser, progress);
			}
			else
			{
				var control = ChromiumWebBrowser.FromBrowser(browser) as CefSharpBrowserControl;

				control?.OnLoadingProgressChange(chromiumWebBrowser, browser, progress);
			}

			base.OnLoadingProgressChange(chromiumWebBrowser, browser, progress);
		}
	}
}
