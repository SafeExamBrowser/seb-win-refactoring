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
	internal class ContextMenuHandlerSwitch : IContextMenuHandler
	{
		public void OnBeforeContextMenu(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
		{
			var control = default(ICefSharpControl);

			if (browser.IsPopup)
			{
				control = ChromiumHostControl.FromBrowser(browser) as CefSharpPopupControl;
			}
			else
			{
				control = ChromiumWebBrowser.FromBrowser(browser) as CefSharpBrowserControl;
			}

			control?.OnBeforeContextMenu(webBrowser, browser, frame, parameters, model);
		}

		public bool OnContextMenuCommand(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
		{
			var args = new GenericEventArgs();
			var control = default(ICefSharpControl);

			if (browser.IsPopup)
			{
				control = ChromiumHostControl.FromBrowser(browser) as CefSharpPopupControl;
			}
			else
			{
				control = ChromiumWebBrowser.FromBrowser(browser) as CefSharpBrowserControl;
			}

			control?.OnContextMenuCommand(webBrowser, browser, frame, parameters, commandId, eventFlags, args);

			return args.Value;
		}

		public void OnContextMenuDismissed(IWebBrowser webBrowser, IBrowser browser, IFrame frame)
		{
			var control = default(ICefSharpControl);

			if (browser.IsPopup)
			{
				control = ChromiumHostControl.FromBrowser(browser) as CefSharpPopupControl;
			}
			else
			{
				control = ChromiumWebBrowser.FromBrowser(browser) as CefSharpBrowserControl;
			}

			control?.OnContextMenuDismissed(webBrowser, browser, frame);
		}

		public bool RunContextMenu(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
		{
			var args = new GenericEventArgs();
			var control = default(ICefSharpControl);

			if (browser.IsPopup)
			{
				control = ChromiumHostControl.FromBrowser(browser) as CefSharpPopupControl;
			}
			else
			{
				control = ChromiumWebBrowser.FromBrowser(browser) as CefSharpBrowserControl;
			}

			control?.OnRunContextMenu(webBrowser, browser, frame, parameters, model, callback, args);

			return args.Value;
		}
	}
}
