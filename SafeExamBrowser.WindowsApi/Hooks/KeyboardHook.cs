/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.InteropServices;
using SafeExamBrowser.WindowsApi.Constants;
using SafeExamBrowser.WindowsApi.Contracts.Events;
using SafeExamBrowser.WindowsApi.Delegates;
using SafeExamBrowser.WindowsApi.Types;

namespace SafeExamBrowser.WindowsApi.Hooks
{
	internal class KeyboardHook
	{
		private bool altPressed, ctrlPressed;
		private KeyboardHookCallback callback;
		private IntPtr handle;
		private HookDelegate hookDelegate;

		internal Guid Id { get; private set; }

		internal KeyboardHook(KeyboardHookCallback callback)
		{
			this.callback = callback;
			this.Id = Guid.NewGuid();
		}

		internal void Attach()
		{
			var process = System.Diagnostics.Process.GetCurrentProcess();
			var module = process.MainModule;
			var moduleHandle = Kernel32.GetModuleHandle(module.ModuleName);

			// IMPORTANT:
			// Ensures that the hook delegate does not get garbage collected prematurely, as it will be passed to unmanaged code.
			// Not doing so will result in a <c>CallbackOnCollectedDelegate</c> error and subsequent application crash!
			hookDelegate = new HookDelegate(LowLevelKeyboardProc);
			handle = User32.SetWindowsHookEx(HookType.WH_KEYBOARD_LL, hookDelegate, moduleHandle, 0);
		}

		internal bool Detach()
		{
			return User32.UnhookWindowsHookEx(handle);
		}

		private IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode >= 0)
			{
				var keyData = (KBDLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
				var modifier = GetModifiers(keyData, wParam.ToInt32());
				var state = GetState(wParam.ToInt32());

				if (callback((int) keyData.KeyCode, modifier, state))
				{
					return (IntPtr) 1;
				}
			}

			return User32.CallNextHookEx(handle, nCode, wParam, lParam);
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
					return KeyState.Unknown;
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

			if (keyCode == (uint) VirtualKeyCode.LeftControl || keyCode == (uint) VirtualKeyCode.RightControl)
			{
				ctrlPressed = IsPressed(wParam);
			}
			else if (keyCode == (uint) VirtualKeyCode.LeftAlt || keyCode == (uint) VirtualKeyCode.RightAlt)
			{
				altPressed = IsPressed(wParam);
			}

			if (ctrlPressed && altPressed && keyCode == (uint) VirtualKeyCode.Delete)
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
