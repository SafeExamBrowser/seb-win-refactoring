/*
 * Copyright (c) 2024 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using CefSharp;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Browser.Handlers
{
	internal class FocusHandler : IFocusHandler
	{
		private readonly ILogger logger;

		internal FocusHandler(ILogger logger)
		{
			this.logger = logger;
		}

		public void OnGotFocus(IWebBrowser webBrowser, IBrowser browser)
		{
		}

		public bool OnSetFocus(IWebBrowser webBrowser, IBrowser browser, CefFocusSource source)
		{
			return true;
		}

		public void OnTakeFocus(IWebBrowser webBrowser, IBrowser browser, bool next)
		{
		}
	}
}
