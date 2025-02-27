/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using CefSharp;
using CefSharp.Enums;
using CefSharp.WinForms;
using CefSharp.WinForms.Host;
using SafeExamBrowser.Browser.Wrapper.Events;

namespace SafeExamBrowser.Browser.Wrapper.Handlers
{
	internal class DragHandlerSwitch : IDragHandler
	{
		public bool OnDragEnter(IWebBrowser webBrowser, IBrowser browser, IDragData dragData, DragOperationsMask mask)
		{
			var args = new GenericEventArgs();

			if (browser.IsPopup)
			{
				var control = ChromiumHostControl.FromBrowser(browser) as CefSharpPopupControl;

				control?.OnDragEnter(webBrowser, browser, dragData, mask, args);
			}
			else
			{
				var control = ChromiumWebBrowser.FromBrowser(browser) as CefSharpBrowserControl;

				control?.OnDragEnter(webBrowser, browser, dragData, mask, args);
			}

			return args.Value;
		}

		public void OnDraggableRegionsChanged(IWebBrowser webBrowser, IBrowser browser, IFrame frame, IList<DraggableRegion> regions)
		{
			if (browser.IsPopup)
			{
				var control = ChromiumHostControl.FromBrowser(browser) as CefSharpPopupControl;

				control?.OnDraggableRegionsChanged(webBrowser, browser, frame, regions);
			}
			else
			{
				var control = ChromiumWebBrowser.FromBrowser(browser) as CefSharpBrowserControl;

				control?.OnDraggableRegionsChanged(webBrowser, browser, frame, regions);
			}
		}
	}
}
