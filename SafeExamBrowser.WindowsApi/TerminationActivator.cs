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
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.WindowsApi.Contracts;
using SafeExamBrowser.WindowsApi.Contracts.Events;
using SafeExamBrowser.WindowsApi.Constants;
using SafeExamBrowser.WindowsApi.Delegates;
using SafeExamBrowser.WindowsApi.Types;

namespace SafeExamBrowser.WindowsApi
{
	public class TerminationActivator : ITerminationActivator
	{
		private bool Q, LeftCtrl, RightCtrl, paused;
		private IntPtr handle;
		private HookDelegate hookDelegate;
		private ILogger logger;

		public event TerminationActivatorEventHandler Activated;

		public TerminationActivator(ILogger logger)
		{
			this.logger = logger;
		}

		public void Pause()
		{
			paused = true;
		}

		public void Resume()
		{
			Q = false;
			LeftCtrl = false;
			RightCtrl = false;
			paused = false;
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
			if (nCode >= 0 && !paused)
			{
				var changed = false;
				var keyData = (KBDLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
				var pressed = IsPressed(wParam.ToInt32());

				switch (keyData.KeyCode)
				{
					case (uint) VirtualKeyCode.Q:
						changed = Q != pressed;
						Q = pressed;
						break;
					case (uint) VirtualKeyCode.LeftControl:
						changed = LeftCtrl != pressed;
						LeftCtrl = pressed;
						break;
					case (uint) VirtualKeyCode.RightControl:
						changed = RightCtrl != pressed;
						RightCtrl = pressed;
						break;
				}

				if (Q && (LeftCtrl || RightCtrl) && changed)
				{
					logger.Debug("Detected termination sequence.");
					Activated?.Invoke();

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
