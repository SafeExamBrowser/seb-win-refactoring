/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using CefSharp;
using CefSharp.WinForms;
using CefSharp.WinForms.Host;
using SafeExamBrowser.Browser.Wrapper.Events;

namespace SafeExamBrowser.Browser.Wrapper.Handlers
{
	internal class FocusHandlerSwitch : IFocusHandler
	{
		public void OnGotFocus(IWebBrowser webBrowser, IBrowser browser)
		{
			if (browser.IsPopup)
			{
				var control = ChromiumHostControl.FromBrowser(browser) as CefSharpPopupControl;

				control?.OnGotFocus(webBrowser, browser);
			}
			else
			{
				var control = ChromiumWebBrowser.FromBrowser(browser) as CefSharpBrowserControl;

				control?.OnGotFocus(webBrowser, browser);
			}
		}

		public bool OnSetFocus(IWebBrowser webBrowser, IBrowser browser, CefFocusSource source)
		{
			var args = new GenericEventArgs { Value = false };

			if (browser.IsPopup)
			{
				var control = ChromiumHostControl.FromBrowser(browser) as CefSharpPopupControl;

				control?.OnSetFocus(webBrowser, browser, source, args);
			}
			else
			{
				var control = ChromiumWebBrowser.FromBrowser(browser) as CefSharpBrowserControl;

				control?.OnSetFocus(webBrowser, browser, source, args);
			}

			return args.Value;
		}

		public void OnTakeFocus(IWebBrowser webBrowser, IBrowser browser, bool next)
		{
			if (browser.IsPopup)
			{
				var control = ChromiumHostControl.FromBrowser(browser) as CefSharpPopupControl;

				control?.OnTakeFocus(webBrowser, browser, next);
			}
			else
			{
				var control = ChromiumWebBrowser.FromBrowser(browser) as CefSharpBrowserControl;

				control?.OnTakeFocus(webBrowser, browser, next);
			}
		}
	}
}
