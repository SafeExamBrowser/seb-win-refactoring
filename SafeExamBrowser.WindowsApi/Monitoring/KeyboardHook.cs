/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.InteropServices;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.WindowsApi.Constants;
using SafeExamBrowser.WindowsApi.Types;

namespace SafeExamBrowser.WindowsApi.Monitoring
{
	internal class KeyboardHook
	{
		private const int LEFT_CTRL = 162;
		private const int RIGHT_CTRL = 163;
		private const int LEFT_ALT = 164;
		private const int RIGHT_ALT = 165;
		private const int DELETE = 46;

		private bool altPressed, ctrlPressed;
		private HookProc hookProc;

		internal IntPtr Handle { get; private set; }
		internal IKeyboardInterceptor Interceptor { get; private set; }

		internal KeyboardHook(IKeyboardInterceptor interceptor)
		{
			Interceptor = interceptor;
		}

		internal void Attach()
		{
			var module = Kernel32.GetModuleHandle(null);

			// IMORTANT:
			// Ensures that the hook delegate does not get garbage collected prematurely, as it will be passed to unmanaged code.
			// Not doing so will result in a <c>CallbackOnCollectedDelegate</c> error and subsequent application crash!
			hookProc = new HookProc(LowLevelKeyboardProc);

			Handle = User32.SetWindowsHookEx(HookType.WH_KEYBOARD_LL, hookProc, module, 0);
		}

		internal bool Detach()
		{
			return User32.UnhookWindowsHookEx(Handle);
		}

		private IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode >= 0)
			{
				var keyData = (KBDLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
				var modifier = GetModifiers(keyData, wParam.ToInt32());
				var state = GetState(wParam.ToInt32());

				if (Interceptor.Block((int) keyData.KeyCode, modifier, state))
				{
					return (IntPtr) 1;
				}
			}

			return User32.CallNextHookEx(Handle, nCode, wParam, lParam);
		}

		private KeyState GetState(int wParam)
		{
			switch (wParam)
			{
				case Constant.WM_KEYDOWN:
				case Constant.WM_SYSKEYDOWN:
					return KeyState.Pressed;
				case Constant.WM_KEYUP:
				case Constant.WM_SYSKEYUP:
					return KeyState.Released;
				default:
					return KeyState.None;
			}
		}

		private KeyModifier GetModifiers(KBDLLHOOKSTRUCT keyData, int wParam)
		{
			var modifier = KeyModifier.None;

			TrackCtrlAndAlt(keyData, wParam);

			if (altPressed || keyData.Flags.HasFlag(KBDLLHOOKSTRUCTFlags.LLKHF_ALTDOWN))
			{
				modifier |= KeyModifier.Alt;
			}

			if (ctrlPressed)
			{
				modifier |= KeyModifier.Ctrl;
			}

			return modifier;
		}

		private void TrackCtrlAndAlt(KBDLLHOOKSTRUCT keyData, int wParam)
		{
			var keyCode = keyData.KeyCode;

			if (keyCode == LEFT_CTRL || keyCode == RIGHT_CTRL)
			{
				ctrlPressed = IsPressed(wParam);
			}
			else if (keyCode == LEFT_ALT || keyCode == RIGHT_ALT)
			{
				altPressed = IsPressed(wParam);
			}

			if (ctrlPressed && altPressed && keyCode == DELETE)
			{
				// When the Secure Attention Sequence is pressed, the WM_KEYUP / WM_SYSKEYUP messages for CTRL and ALT get lost...
				ctrlPressed = false;
				altPressed = false;
			}
		}

		private bool IsPressed(int wParam)
		{
			return wParam == Constant.WM_KEYDOWN || wParam == Constant.WM_SYSKEYDOWN;
		}
	}
}
