/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows.Forms;
using CefSharp;
using SafeExamBrowser.UserInterface.Contracts;

namespace SafeExamBrowser.Browser.Handlers
{
	internal class KeyboardHandler : IKeyboardHandler
	{
		internal event ActionRequestedEventHandler FindRequested;
		internal event ActionRequestedEventHandler HomeNavigationRequested;
		internal event ActionRequestedEventHandler ReloadRequested;
		internal event ActionRequestedEventHandler ZoomInRequested;
		internal event ActionRequestedEventHandler ZoomOutRequested;
		internal event ActionRequestedEventHandler ZoomResetRequested;

		public bool OnKeyEvent(IWebBrowser browserControl, IBrowser browser, KeyType type, int keyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey)
		{
			var ctrl = modifiers.HasFlag(CefEventFlags.ControlDown);
			var shift = modifiers.HasFlag(CefEventFlags.ShiftDown);

			if (type == KeyType.KeyUp)
			{
				if (ctrl && keyCode == (int) Keys.F)
				{
					FindRequested?.Invoke();
				}

				if (keyCode == (int) Keys.Home)
				{
					HomeNavigationRequested?.Invoke();
				}

				if ((ctrl && keyCode == (int) Keys.Add) || (ctrl && shift && keyCode == (int) Keys.D1))
				{
					ZoomInRequested?.Invoke();
				}

				if (ctrl && (keyCode == (int) Keys.Subtract || keyCode == (int) Keys.OemMinus))
				{
					ZoomOutRequested?.Invoke();
				}

				if (ctrl && (keyCode == (int) Keys.D0 || keyCode == (int) Keys.NumPad0))
				{
					ZoomResetRequested?.Invoke();
				}
			}

			return false;
		}

		public bool OnPreKeyEvent(IWebBrowser browserControl, IBrowser browser, KeyType type, int keyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey, ref bool isKeyboardShortcut)
		{
			if (type == KeyType.KeyUp && keyCode == (int) Keys.F5)
			{
				ReloadRequested?.Invoke();

				return true;
			}

			return false;
		}
	}
}
