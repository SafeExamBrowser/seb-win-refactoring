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
	internal class MouseHook
	{
		private MouseHookCallback callback;
		private IntPtr handle;
		private HookDelegate hookDelegate;

		internal Guid Id { get; private set; }

		internal MouseHook(MouseHookCallback callback)
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
			hookDelegate = new HookDelegate(LowLevelMouseProc);
			handle = User32.SetWindowsHookEx(HookType.WH_MOUSE_LL, hookDelegate, moduleHandle, 0);
		}

		internal bool Detach()
		{
			return User32.UnhookWindowsHookEx(handle);
		}

		private IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode >= 0 && !Ignore(wParam.ToInt32()))
			{
				var mouseData = (MSLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
				var button = GetButton(wParam.ToInt32());
				var state = GetState(wParam.ToInt32());
				var info = GetInfo(mouseData);

				if (callback(button, state, info))
				{
					return (IntPtr) 1;
				}
			}

			return User32.CallNextHookEx(handle, nCode, wParam, lParam);
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
				case Constant.WM_XBUTTONDOWN:
				case Constant.WM_XBUTTONUP:
					return MouseButton.Auxiliary;
				default:
					return MouseButton.Unknown;
			}
		}

		private MouseInformation GetInfo(MSLLHOOKSTRUCT mouseData)
		{
			var info = new MouseInformation();
			var extraInfo = mouseData.DwExtraInfo.ToUInt32();

			info.IsTouch = (extraInfo & Constant.MOUSEEVENTF_MASK) == Constant.MOUSEEVENTF_FROMTOUCH;
			info.X = mouseData.Point.X;
			info.Y = mouseData.Point.Y;

			return info;
		}

		private MouseButtonState GetState(int wParam)
		{
			switch (wParam)
			{
				case Constant.WM_LBUTTONDOWN:
				case Constant.WM_MBUTTONDOWN:
				case Constant.WM_RBUTTONDOWN:
				case Constant.WM_XBUTTONDOWN:
					return MouseButtonState.Pressed;
				case Constant.WM_LBUTTONUP:
				case Constant.WM_MBUTTONUP:
				case Constant.WM_RBUTTONUP:
				case Constant.WM_XBUTTONUP:
					return MouseButtonState.Released;
				default:
					return MouseButtonState.Unknown;
			}
		}
	}
}
