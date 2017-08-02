/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using CefSharp;
using SafeExamBrowser.Contracts.I18n;
using IBrowserSettings = SafeExamBrowser.Contracts.Configuration.Settings.IBrowserSettings;

namespace SafeExamBrowser.Browser.Handlers
{
	/// <remarks>
	/// See https://cefsharp.github.io/api/57.0.0/html/T_CefSharp_IContextMenuHandler.htm.
	/// </remarks>
	internal class BrowserContextMenuHandler : IContextMenuHandler
	{
		private const int DEV_CONSOLE_COMMAND = (int) CefMenuCommand.UserFirst + 1;

		private IBrowserSettings settings;
		private IText text;

		public BrowserContextMenuHandler(IBrowserSettings settings, IText text)
		{
			this.settings = settings;
			this.text = text;
		}

		public void OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
		{
			model.Clear();

			if (settings.AllowDeveloperConsole)
			{
				model.AddItem((CefMenuCommand) DEV_CONSOLE_COMMAND, text.Get(Key.Browser_ShowDeveloperConsole));
			}
		}

		public bool OnContextMenuCommand(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
		{
			if ((int) commandId == DEV_CONSOLE_COMMAND)
			{
				browser.ShowDevTools();

				return true;
			}

			return false;
		}

		public void OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame)
		{
		}

		public bool RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
		{
			return false;
		}
	}
}
