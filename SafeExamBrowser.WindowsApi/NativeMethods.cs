/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
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
using SafeExamBrowser.WindowsApi.Constants;
using SafeExamBrowser.WindowsApi.Delegates;
using SafeExamBrowser.WindowsApi.Monitoring;
using SafeExamBrowser.WindowsApi.Types;

namespace SafeExamBrowser.WindowsApi
{
	public class NativeMethods : INativeMethods
	{
		private ConcurrentDictionary<IntPtr, EventDelegate> EventDelegates = new ConcurrentDictionary<IntPtr, EventDelegate>();
		private ConcurrentDictionary<IntPtr, KeyboardHook> KeyboardHooks = new ConcurrentDictionary<IntPtr, KeyboardHook>();
		private ConcurrentDictionary<IntPtr, MouseHook> MouseHooks = new ConcurrentDictionary<IntPtr, MouseHook>();

		/// <summary>
		/// Upon finalization, unregister all active system events and hooks...
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

			foreach (var handle in MouseHooks.Keys)
			{
				User32.UnhookWindowsHookEx(handle);
			}
		}

		public void DeregisterKeyboardHook(IKeyboardInterceptor interceptor)
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

		public void DeregisterMouseHook(IMouseInterceptor interceptor)
		{
			var hook = MouseHooks.Values.FirstOrDefault(h => h.Interceptor == interceptor);

			if (hook != null)
			{
				var success = hook.Detach();

				if (!success)
				{
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}

				MouseHooks.TryRemove(hook.Handle, out MouseHook h);
			}
		}

		public void DeregisterSystemEvent(IntPtr handle)
		{
			var success = User32.UnhookWinEvent(handle);

			if (!success)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}

			EventDelegates.TryRemove(handle, out EventDelegate d);
		}

		public void EmptyClipboard()
		{
			var success = true;

			success &= User32.OpenClipboard(IntPtr.Zero);
			success &= User32.EmptyClipboard();
			success &= User32.CloseClipboard();

			if (!success)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
		}

		public IEnumerable<IntPtr> GetOpenWindows()
		{
			var windows = new List<IntPtr>();

			bool EnumWindows(IntPtr hWnd, IntPtr lParam)
			{
				if (hWnd != GetShellWindowHandle() && User32.IsWindowVisible(hWnd) && User32.GetWindowTextLength(hWnd) > 0)
				{
					windows.Add(hWnd);
				}

				return true;
			}

			var success = User32.EnumWindows(EnumWindows, IntPtr.Zero);

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

		public string GetWallpaperPath()
		{
			const int MAX_PATH = 260;
			var buffer = new String('\0', MAX_PATH);
			var success = User32.SystemParametersInfo(SPI.GETDESKWALLPAPER, buffer.Length, buffer, 0);

			if (!success)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}

			var path = buffer.Substring(0, buffer.IndexOf('\0'));

			return path;
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

		public IBounds GetWorkingArea()
		{
			var workingArea = new RECT();
			var success = User32.SystemParametersInfo(SPI.GETWORKAREA, 0, ref workingArea, SPIF.NONE);

			if (!success)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}

			return workingArea.ToBounds();
		}

		public bool HideWindow(IntPtr window)
		{
			return User32.ShowWindow(window, (int) ShowWindowCommand.Hide);
		}

		public void MinimizeAllOpenWindows()
		{
			var handle = GetShellWindowHandle();

			User32.SendMessage(handle, Constant.WM_COMMAND, (IntPtr) Constant.MIN_ALL, IntPtr.Zero);
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

		public void PreventSleepMode()
		{
			Kernel32.SetThreadExecutionState(EXECUTION_STATE.CONTINUOUS | EXECUTION_STATE.DISPLAY_REQUIRED | EXECUTION_STATE.SYSTEM_REQUIRED);
		}

		public void RegisterKeyboardHook(IKeyboardInterceptor interceptor)
		{
			var hook = new KeyboardHook(interceptor);

			hook.Attach();

			KeyboardHooks[hook.Handle] = hook;
		}

		public void RegisterMouseHook(IMouseInterceptor interceptor)
		{
			var hook = new MouseHook(interceptor);

			hook.Attach();

			MouseHooks[hook.Handle] = hook;
		}

		public IntPtr RegisterSystemForegroundEvent(Action<IntPtr> callback)
		{
			void evenDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
			{
				callback(hwnd);
			}

			var handle = User32.SetWinEventHook(Constant.EVENT_SYSTEM_FOREGROUND, Constant.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, evenDelegate, 0, 0, Constant.WINEVENT_OUTOFCONTEXT);

			// IMORTANT:
			// Ensures that the event delegate does not get garbage collected prematurely, as it will be passed to unmanaged code.
			// Not doing so will result in a <c>CallbackOnCollectedDelegate</c> error and subsequent application crash!
			EventDelegates[handle] = evenDelegate;

			return handle;
		}

		public IntPtr RegisterSystemCaptureStartEvent(Action<IntPtr> callback)
		{
			void eventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
			{
				callback(hwnd);
			}

			var handle = User32.SetWinEventHook(Constant.EVENT_SYSTEM_CAPTURESTART, Constant.EVENT_SYSTEM_CAPTURESTART, IntPtr.Zero, eventDelegate, 0, 0, Constant.WINEVENT_OUTOFCONTEXT);

			// IMORTANT:
			// Ensures that the event delegate does not get garbage collected prematurely, as it will be passed to unmanaged code.
			// Not doing so will result in a <c>CallbackOnCollectedDelegate</c> error and subsequent application crash!
			EventDelegates[handle] = eventDelegate;

			return handle;
		}

		public void RemoveWallpaper()
		{
			SetWallpaper(string.Empty);
		}

		public void RestoreWindow(IntPtr window)
		{
			User32.ShowWindow(window, (int)ShowWindowCommand.Restore);
		}

		public bool ResumeThread(int threadId)
		{
			const int FAILURE = -1;
			var handle = Kernel32.OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint) threadId);

			if (handle == IntPtr.Zero)
			{
				return false;
			}

			try
			{
				var result = Kernel32.ResumeThread(handle);
				var success = result != FAILURE;

				return success;
			}
			finally
			{
				Kernel32.CloseHandle(handle);
			}
		}

		public void SendCloseMessageTo(IntPtr window)
		{
			User32.SendMessage(window, Constant.WM_SYSCOMMAND, (IntPtr) SystemCommand.CLOSE, IntPtr.Zero);
		}

		public void SetWallpaper(string filePath)
		{
			var success = User32.SystemParametersInfo(SPI.SETDESKWALLPAPER, 0, filePath, SPIF.UPDATEANDCHANGE);

			if (!success)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
		}

		public void SetWorkingArea(IBounds bounds)
		{
			var workingArea = new RECT { Left = bounds.Left, Top = bounds.Top, Right = bounds.Right, Bottom = bounds.Bottom };
			var success = User32.SystemParametersInfo(SPI.SETWORKAREA, 0, ref workingArea, SPIF.NONE);

			if (!success)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
		}

		public bool SuspendThread(int threadId)
		{
			const int FAILURE = -1;
			var handle = Kernel32.OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint) threadId);

			if (handle == IntPtr.Zero)
			{
				return false;
			}

			try
			{
				var result = Kernel32.SuspendThread(handle);
				var success = result != FAILURE;

				return success;
			}
			finally
			{
				Kernel32.CloseHandle(handle);
			}
		}
	}
}
