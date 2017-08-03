/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.InteropServices;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.WindowsApi.Constants;

namespace SafeExamBrowser.WindowsApi.Monitoring
{
	internal class KeyboardHook
	{
		internal IntPtr Handle { get; private set; }
		internal IKeyboardInterceptor Interceptor { get; private set; }

		internal KeyboardHook(IKeyboardInterceptor interceptor)
		{
			Interceptor = interceptor;
		}

		internal void Attach()
		{
			var module = Kernel32.GetModuleHandle(null);

			Handle = User32.SetWindowsHookEx(HookType.WH_KEYBOARD_LL, LowLevelKeyboardProc, module, 0);
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
				var modifier = GetModifiers(keyData);

				if (Interceptor.Block(keyData.KeyCode, modifier))
				{
					return (IntPtr) 1;
				}
			}

			return User32.CallNextHookEx(Handle, nCode, wParam, lParam);
		}

		private KeyModifier GetModifiers(KBDLLHOOKSTRUCT keyData)
		{
			var modifier = KeyModifier.None;

			if ((keyData.Flags & KBDLLHOOKSTRUCTFlags.LLKHF_ALTDOWN) == KBDLLHOOKSTRUCTFlags.LLKHF_ALTDOWN)
			{
				modifier |= KeyModifier.Alt;
			}

			if (keyData.Flags == 0)
			{
				modifier |= KeyModifier.Ctrl;
			}

			return modifier;
		}
	}
}
