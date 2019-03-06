/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.InteropServices;
using System.Threading;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface.Shell;
using SafeExamBrowser.Contracts.UserInterface.Shell.Events;
using SafeExamBrowser.WindowsApi.Constants;
using SafeExamBrowser.WindowsApi.Delegates;
using SafeExamBrowser.WindowsApi.Types;

namespace SafeExamBrowser.WindowsApi
{
	public class KeyboardActivator : IActionCenterActivator
	{
		private bool A, LeftWindows;
		private IntPtr handle;
		private HookDelegate hookDelegate;
		private ILogger logger;

		public event ActivatorEventHandler Activate { add { } remove { } }
		public event ActivatorEventHandler Deactivate { add { } remove { } }
		public event ActivatorEventHandler Toggle;

		public KeyboardActivator(ILogger logger)
		{
			this.logger = logger;
		}

		public void Start()
		{
			var hookReadyEvent = new AutoResetEvent(false);
			var hookThread = new Thread(() =>
			{
				var sleepEvent = new AutoResetEvent(false);
				var process = System.Diagnostics.Process.GetCurrentProcess();
				var module = process.MainModule;
				var moduleHandle = Kernel32.GetModuleHandle(module.ModuleName);

				// IMPORTANT:
				// Ensures that the hook delegate does not get garbage collected prematurely, as it will be passed to unmanaged code.
				// Not doing so will result in a <c>CallbackOnCollectedDelegate</c> error and subsequent application crash!
				hookDelegate = new HookDelegate(LowLevelKeyboardProc);

				handle = User32.SetWindowsHookEx(HookType.WH_KEYBOARD_LL, hookDelegate, moduleHandle, 0);
				hookReadyEvent.Set();

				while (true)
				{
					sleepEvent.WaitOne();
				}
			});

			hookThread.SetApartmentState(ApartmentState.STA);
			hookThread.IsBackground = true;
			hookThread.Start();

			hookReadyEvent.WaitOne();
		}

		public void Stop()
		{
			User32.UnhookWindowsHookEx(handle);
		}

		private IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode >= 0)
			{
				var changed = false;
				var keyData = (KBDLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
				var pressed = IsPressed(wParam.ToInt32());

				switch (keyData.KeyCode)
				{
					case (uint) VirtualKeyCode.A:
						changed = A != pressed;
						A = pressed;
						break;
					case (uint) VirtualKeyCode.LeftWindows:
						changed = LeftWindows != pressed;
						LeftWindows = pressed;
						break;
				}

				if (A && LeftWindows && changed)
				{
					logger.Debug("Detected toggle sequence for action center.");
					Toggle?.Invoke();

					return (IntPtr) 1;
				}
			}

			return User32.CallNextHookEx(handle, nCode, wParam, lParam);
		}

		private bool IsPressed(int wParam)
		{
			return wParam == Constant.WM_KEYDOWN || wParam == Constant.WM_SYSKEYDOWN;
		}
	}
}
