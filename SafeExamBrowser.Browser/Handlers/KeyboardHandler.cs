/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows.Forms;
using CefSharp;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Browser.Handlers
{
	/// <remarks>
	/// See https://cefsharp.github.io/api/67.0.0/html/T_CefSharp_IKeyboardHandler.htm.
	/// </remarks>
	internal class KeyboardHandler : IKeyboardHandler
	{
		public event ActionRequestedEventHandler ReloadRequested;
		public event ActionRequestedEventHandler ZoomInRequested;
		public event ActionRequestedEventHandler ZoomOutRequested;
		public event ActionRequestedEventHandler ZoomResetRequested;

		public bool OnKeyEvent(IWebBrowser browserControl, IBrowser browser, KeyType type, int keyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey)
		{
			var ctrl = modifiers.HasFlag(CefEventFlags.ControlDown);
			var shift = modifiers.HasFlag(CefEventFlags.ShiftDown);

			if (type == KeyType.KeyUp && ((keyCode == (int)Keys.Add && ctrl) || (keyCode == (int)Keys.D1 && ctrl && shift)))
			{
				ZoomInRequested?.Invoke();
			}

			if (type == KeyType.KeyUp && (keyCode == (int) Keys.Subtract || keyCode == (int) Keys.OemMinus) && ctrl)
			{
				ZoomOutRequested?.Invoke();
			}

			if (type == KeyType.KeyUp && (keyCode == (int) Keys.D0 || keyCode == (int) Keys.NumPad0) && ctrl)
			{
				ZoomResetRequested?.Invoke();
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
