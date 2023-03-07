/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows.Forms;
using CefSharp;
using SafeExamBrowser.Browser.Contracts.Events;
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
		internal event ActionRequestedEventHandler FocusAddressBarRequested;
		internal event TabPressedEventHandler TabPressed;

		private int? currentKeyDown = null;

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

				if (ctrl && keyCode == (int) Keys.L)
				{
					FocusAddressBarRequested?.Invoke();
				}

				if ((ctrl && keyCode == (int) Keys.Add) || (ctrl && keyCode == (int) Keys.Oemplus) || (ctrl && shift && keyCode == (int) Keys.D1))
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

				if (keyCode == (int) Keys.Tab && keyCode == currentKeyDown)
				{
					TabPressed?.Invoke(shift);
				}
			}

			currentKeyDown = null;
			return false;
		}

		public bool OnPreKeyEvent(IWebBrowser browserControl, IBrowser browser, KeyType type, int keyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey, ref bool isKeyboardShortcut)
		{
			if (type == KeyType.KeyUp && keyCode == (int) Keys.F5)
			{
				ReloadRequested?.Invoke();

				return true;
			}

			if (type == KeyType.RawKeyDown || type == KeyType.KeyDown)
			{
				currentKeyDown = keyCode;
			}

			return false;
		}
	}
}
