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
	internal class JavaScriptDialogHandlerSwitch : IJsDialogHandler
	{
		public bool OnBeforeUnloadDialog(IWebBrowser webBrowser, IBrowser browser, string message, bool isReload, IJsDialogCallback callback)
		{
			var args = new GenericEventArgs { Value = false };

			if (browser.IsPopup)
			{
				var control = ChromiumHostControl.FromBrowser(browser) as CefSharpPopupControl;

				control?.OnBeforeUnloadDialog(webBrowser, browser, message, isReload, callback, args);
			}
			else
			{
				var control = ChromiumWebBrowser.FromBrowser(browser) as CefSharpBrowserControl;

				control?.OnBeforeUnloadDialog(webBrowser, browser, message, isReload, callback, args);
			}

			return args.Value;
		}

		public void OnDialogClosed(IWebBrowser webBrowser, IBrowser browser)
		{
			if (browser.IsPopup)
			{
				var control = ChromiumHostControl.FromBrowser(browser) as CefSharpPopupControl;

				control?.OnDialogClosed(webBrowser, browser);
			}
			else
			{
				var control = ChromiumWebBrowser.FromBrowser(browser) as CefSharpBrowserControl;

				control?.OnDialogClosed(webBrowser, browser);
			}
		}

		public bool OnJSDialog(IWebBrowser webBrowser, IBrowser browser, string originUrl, CefJsDialogType type, string message, string promptText, IJsDialogCallback callback, ref bool suppress)
		{
			var args = new GenericEventArgs { Value = false };

			if (browser.IsPopup)
			{
				var control = ChromiumHostControl.FromBrowser(browser) as CefSharpPopupControl;

				control?.OnJavaScriptDialog(webBrowser, browser, originUrl, type, message, promptText, callback, ref suppress, args);
			}
			else
			{
				var control = ChromiumWebBrowser.FromBrowser(browser) as CefSharpBrowserControl;

				control?.OnJavaScriptDialog(webBrowser, browser, originUrl, type, message, promptText, callback, ref suppress, args);
			}

			return args.Value;
		}

		public void OnResetDialogState(IWebBrowser webBrowser, IBrowser browser)
		{
			if (browser.IsPopup)
			{
				var control = ChromiumHostControl.FromBrowser(browser) as CefSharpPopupControl;

				control?.OnResetDialogState(webBrowser, browser);
			}
			else
			{
				var control = ChromiumWebBrowser.FromBrowser(browser) as CefSharpBrowserControl;

				control?.OnResetDialogState(webBrowser, browser);
			}
		}
	}
}
