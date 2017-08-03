/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.Contracts.WindowsApi;
using SafeExamBrowser.Contracts.WindowsApi.Types;
using SafeExamBrowser.WindowsApi.Constants;
using SafeExamBrowser.WindowsApi.Monitoring;

namespace SafeExamBrowser.WindowsApi
{
	public class NativeMethods : INativeMethods
	{
		private ConcurrentDictionary<IntPtr, EventProc> EventDelegates = new ConcurrentDictionary<IntPtr, EventProc>();
		private ConcurrentDictionary<IntPtr, KeyboardHook> KeyboardHooks = new ConcurrentDictionary<IntPtr, KeyboardHook>();

		/// <summary>
		/// Upon finalization, unregister all system events and hooks...
		/// </summary>
		~NativeMethods()
		{
			foreach (var handle in EventDelegates.Keys)
			{
				User32.UnhookWinEvent(handle);
			}

			foreach (var handle in KeyboardHooks.Keys)
			{
				User32.UnhookWindowsHookEx(handle);
			}
		}

		public IEnumerable<IntPtr> GetOpenWindows()
		{
			var windows = new List<IntPtr>();
			var success = User32.EnumWindows(delegate (IntPtr hWnd, IntPtr lParam)
			{
				if (hWnd != GetShellWindowHandle() && User32.IsWindowVisible(hWnd) && User32.GetWindowTextLength(hWnd) > 0)
				{
					windows.Add(hWnd);
				}

				return true;
			}, IntPtr.Zero);

			if (!success)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}

			return windows;
		}

		public uint GetProcessIdFor(IntPtr window)
		{
			User32.GetWindowThreadProcessId(window, out uint processId);

			return processId;
		}

		public IntPtr GetShellWindowHandle()
		{
			return User32.FindWindow("Shell_TrayWnd", null);
		}

		public uint GetShellProcessId()
		{
			var handle = GetShellWindowHandle();
			var threadId = User32.GetWindowThreadProcessId(handle, out uint processId);

			return processId;
		}

		public string GetWindowTitle(IntPtr window)
		{
			var length = User32.GetWindowTextLength(window);

			if (length > 0)
			{
				var builder = new StringBuilder(length);

				User32.GetWindowText(window, builder, length + 1);

				return builder.ToString();
			}

			return string.Empty;
		}

		public RECT GetWorkingArea()
		{
			var workingArea = new RECT();
			var success = User32.SystemParametersInfo(SPI.GETWORKAREA, 0, ref workingArea, SPIF.NONE);

			if (!success)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}

			return workingArea;
		}

		public bool HideWindow(IntPtr window)
		{
			return User32.ShowWindow(window, (int) ShowWindowCommand.Hide);
		}

		public void MinimizeAllOpenWindows()
		{
			var handle = GetShellWindowHandle();

			User32.SendMessage(handle, Constant.WM_COMMAND, (IntPtr)Constant.MIN_ALL, IntPtr.Zero);
		}

		public void PostCloseMessageToShell()
		{
			// NOTE: The close message 0x5B4 posted to the shell is undocumented and not officially supported:
			// https://stackoverflow.com/questions/5689904/gracefully-exit-explorer-programmatically/5705965#5705965

			var handle = GetShellWindowHandle();
			var success = User32.PostMessage(handle, 0x5B4, IntPtr.Zero, IntPtr.Zero);

			if (!success)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
		}

		public void RegisterKeyboardHook(IKeyboardInterceptor interceptor)
		{
			var hook = new KeyboardHook(interceptor);

			hook.Attach();

			// IMORTANT:
			// Ensures that the hook does not get garbage collected prematurely, as it will be passed to unmanaged code.
			// Not doing so will result in a <c>CallbackOnCollectedDelegate</c> error and subsequent application crash!
			KeyboardHooks[hook.Handle] = hook;
		}

		public IntPtr RegisterSystemForegroundEvent(Action<IntPtr> callback)
		{
			EventProc eventProc = (IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) =>
			{
				callback(hwnd);
			};

			var handle = User32.SetWinEventHook(Constant.EVENT_SYSTEM_FOREGROUND, Constant.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, eventProc, 0, 0, Constant.WINEVENT_OUTOFCONTEXT);

			// IMORTANT:
			// Ensures that the event delegate does not get garbage collected prematurely, as it will be passed to unmanaged code.
			// Not doing so will result in a <c>CallbackOnCollectedDelegate</c> error and subsequent application crash!
			EventDelegates[handle] = eventProc;

			return handle;
		}

		public IntPtr RegisterSystemCaptureStartEvent(Action<IntPtr> callback)
		{
			EventProc eventProc = (IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) =>
			{
				callback(hwnd);
			};

			var handle = User32.SetWinEventHook(Constant.EVENT_SYSTEM_CAPTURESTART, Constant.EVENT_SYSTEM_CAPTURESTART, IntPtr.Zero, eventProc, 0, 0, Constant.WINEVENT_OUTOFCONTEXT);

			// IMORTANT:
			// Ensures that the event delegate does not get garbage collected prematurely, as it will be passed to unmanaged code.
			// Not doing so will result in a <c>CallbackOnCollectedDelegate</c> error and subsequent application crash!
			EventDelegates[handle] = eventProc;

			return handle;
		}

		public void RestoreWindow(IntPtr window)
		{
			User32.ShowWindow(window, (int)ShowWindowCommand.Restore);
		}

		public void SendCloseMessageTo(IntPtr window)
		{
			User32.SendMessage(window, Constant.WM_SYSCOMMAND, (IntPtr) SystemCommand.CLOSE, IntPtr.Zero);
		}

		public void SetWorkingArea(RECT bounds)
		{
			var success = User32.SystemParametersInfo(SPI.SETWORKAREA, 0, ref bounds, SPIF.UPDATEANDCHANGE);

			if (!success)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
		}

		public void UnregisterKeyboardHook(IKeyboardInterceptor interceptor)
		{
			var hook = KeyboardHooks.Values.FirstOrDefault(h => h.Interceptor == interceptor);

			if (hook != null)
			{
				var success = hook.Detach();

				if (!success)
				{
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}

				KeyboardHooks.TryRemove(hook.Handle, out KeyboardHook h);
			}
		}

		public void UnregisterSystemEvent(IntPtr handle)
		{
			var success = User32.UnhookWinEvent(handle);

			if (!success)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}

			EventDelegates.TryRemove(handle, out EventProc d);
		}
	}
}
