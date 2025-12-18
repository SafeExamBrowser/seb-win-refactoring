/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Browser.Contracts.Events;
using SafeExamBrowser.Browser.Handlers;

namespace SafeExamBrowser.Browser.Responsibilities.Window
{
	internal class KeyboardResponsibility : WindowResponsibility
	{
		private readonly KeyboardHandler keyboardHandler;

		internal event LoseFocusRequestedEventHandler LoseFocusRequested;

		public KeyboardResponsibility(BrowserWindowContext context, KeyboardHandler keyboardHandler) : base(context)
		{
			this.keyboardHandler = keyboardHandler;
		}

		public override void Assume(WindowTask task)
		{
			if (task == WindowTask.RegisterEvents)
			{
				RegisterEvents();
			}
		}

		private void RegisterEvents()
		{
			keyboardHandler.FindRequested += KeyboardHandler_FindRequested;
			keyboardHandler.FocusAddressBarRequested += KeyboardHandler_FocusAddressBarRequested;
			keyboardHandler.HomeNavigationRequested += HomeNavigationRequested;
			keyboardHandler.ReloadRequested += ReloadRequested;
			keyboardHandler.TabPressed += KeyboardHandler_TabPressed;
		}

		private void KeyboardHandler_FindRequested()
		{
			if (Settings.AllowFind)
			{
				Window.ShowFindbar();
			}
		}

		private void KeyboardHandler_FocusAddressBarRequested()
		{
			Window.FocusAddressBar();
		}

		private void KeyboardHandler_TabPressed(bool shiftPressed)
		{
			Control.ExecuteJavaScript("document.activeElement.tagName", result =>
			{
				if (result.Result is string tagName && tagName?.ToUpper() == "BODY")
				{
					// This means the user is now at the start of the focus / tabIndex chain in the website.
					if (shiftPressed)
					{
						Window.FocusToolbar(!shiftPressed);
					}
					else
					{
						LoseFocusRequested?.Invoke(true);
					}
				}
			});
		}
	}
}
