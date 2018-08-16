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
using SafeExamBrowser.WindowsApi.Delegates;
using SafeExamBrowser.WindowsApi.Types;

namespace SafeExamBrowser.WindowsApi.Monitoring
{
	internal class MouseHook
	{
		private HookDelegate hookProc;

		internal IntPtr Handle { get; private set; }
		internal IMouseInterceptor Interceptor { get; private set; }

		internal MouseHook(IMouseInterceptor interceptor)
		{
			Interceptor = interceptor;
		}

		internal void Attach()
		{
			var module = Kernel32.GetModuleHandle(null);

			// IMORTANT:
			// Ensures that the hook delegate does not get garbage collected prematurely, as it will be passed to unmanaged code.
			// Not doing so will result in a <c>CallbackOnCollectedDelegate</c> error and subsequent application crash!
			hookProc = new HookDelegate(LowLevelMouseProc);

			Handle = User32.SetWindowsHookEx(HookType.WH_MOUSE_LL, hookProc, module, 0);
		}

		internal bool Detach()
		{
			return User32.UnhookWindowsHookEx(Handle);
		}

		private IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode >= 0 && !Ignore(wParam.ToInt32()))
			{
				var mouseData = (MSLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
				var button = GetButton(wParam.ToInt32());
				var state = GetState(wParam.ToInt32());

				if (Interceptor.Block(button, state))
				{
					return (IntPtr) 1;
				}
			}

			return User32.CallNextHookEx(Handle, nCode, wParam, lParam);
		}

		private bool Ignore(int wParam)
		{
			// For performance reasons, ignore mouse movement and wheel rotation...
			return wParam == Constant.WM_MOUSEMOVE || wParam == Constant.WM_MOUSEWHEEL;
		}

		private MouseButton GetButton(int wParam)
		{
			switch (wParam)
			{
				case Constant.WM_LBUTTONDOWN:
				case Constant.WM_LBUTTONUP:
					return MouseButton.Left;
				case Constant.WM_MBUTTONDOWN:
				case Constant.WM_MBUTTONUP:
					return MouseButton.Middle;
				case Constant.WM_RBUTTONDOWN:
				case Constant.WM_RBUTTONUP:
					return MouseButton.Right;
				default:
					return MouseButton.None;
			}
		}

		private KeyState GetState(int wParam)
		{
			switch (wParam)
			{
				case Constant.WM_LBUTTONDOWN:
				case Constant.WM_MBUTTONDOWN:
				case Constant.WM_RBUTTONDOWN:
					return KeyState.Pressed;
				case Constant.WM_LBUTTONUP:
				case Constant.WM_MBUTTONUP:
				case Constant.WM_RBUTTONUP:
					return KeyState.Released;
				default:
					return KeyState.None;
			}
		}
	}
}
