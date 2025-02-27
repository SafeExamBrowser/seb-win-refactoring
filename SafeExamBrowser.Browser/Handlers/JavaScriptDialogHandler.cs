/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Threading.Tasks;
using CefSharp;
using SafeExamBrowser.Browser.Events;

namespace SafeExamBrowser.Browser.Handlers
{
	internal class JavaScriptDialogHandler : IJsDialogHandler
	{
		internal event JavaScriptDialogRequestedEventHandler DialogRequested;

		public bool OnBeforeUnloadDialog(IWebBrowser webBrowser, IBrowser browser, string message, bool isReload, IJsDialogCallback callback)
		{
			var args = new JavaScriptDialogRequestedEventArgs
			{
				Type = isReload ? JavaScriptDialogType.Reload : JavaScriptDialogType.LeavePage
			};

			Task.Run(() =>
			{
				DialogRequested?.Invoke(args);

				using (callback)
				{
					callback.Continue(args.Success);
				}
			});

			return true;
		}

		public void OnDialogClosed(IWebBrowser webBrowser, IBrowser browser)
		{
		}

		public bool OnJSDialog(IWebBrowser webBrowser, IBrowser browser, string originUrl, CefJsDialogType type, string message, string promptText, IJsDialogCallback callback, ref bool suppress)
		{
			return false;
		}

		public void OnResetDialogState(IWebBrowser webBrowser, IBrowser browser)
		{
		}
	}
}
