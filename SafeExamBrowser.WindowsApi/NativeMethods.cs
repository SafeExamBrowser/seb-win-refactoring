/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
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
using System.Threading;
using SafeExamBrowser.WindowsApi.Constants;
using SafeExamBrowser.WindowsApi.Contracts;
using SafeExamBrowser.WindowsApi.Contracts.Events;
using SafeExamBrowser.WindowsApi.Hooks;
using SafeExamBrowser.WindowsApi.Types;

namespace SafeExamBrowser.WindowsApi
{
	public class NativeMethods : INativeMethods
	{
		private readonly ConcurrentDictionary<Guid, KeyboardHook> KeyboardHooks = new ConcurrentDictionary<Guid, KeyboardHook>();
		private readonly ConcurrentDictionary<Guid, MouseHook> MouseHooks = new ConcurrentDictionary<Guid, MouseHook>();
		private readonly ConcurrentDictionary<Guid, SystemHook> SystemHooks = new ConcurrentDictionary<Guid, SystemHook>();

		/// <summary>
		/// Upon finalization, unregister all active system events and hooks...
		/// </summary>
		~NativeMethods()
		{
			foreach (var hook in SystemHooks.Values)
			{
				hook.Detach();
			}

			foreach (var hook in KeyboardHooks.Values)
			{
				hook.Detach();
			}

			foreach (var hook in MouseHooks.Values)
			{
				hook.Detach();
			}
		}

		public void ActivateWindow(IntPtr handle)
		{
			var placement = new WINDOWPLACEMENT();

			User32.BringWindowToTop(handle);
			User32.SetForegroundWindow(handle);

			placement.length = Marshal.SizeOf(placement);
			User32.GetWindowPlacement(handle, ref placement);

			if (placement.showCmd == (int) ShowWindowCommand.ShowMinimized)
			{
				User32.ShowWindowAsync(handle, (int) ShowWindowCommand.Restore);
			}
		}

		public void DeregisterKeyboardHook(Guid hookId)
		{
			var hook = KeyboardHooks.Values.FirstOrDefault(h => h.Id == hookId);

			if (hook != null)
			{
				var success = hook.Detach();

				if (!success)
				{
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}

				KeyboardHooks.TryRemove(hookId, out _);
			}
		}

		public void DeregisterMouseHook(Guid hookId)
		{
			var hook = MouseHooks.Values.FirstOrDefault(h => h.Id == hookId);

			if (hook != null)
			{
				var success = hook.Detach();

				if (!success)
				{
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}

				MouseHooks.TryRemove(hookId, out _);
			}
		}

		public void DeregisterSystemEventHook(Guid hookId)
		{
			var hook = SystemHooks.Values.FirstOrDefault(h => h.Id == hookId);

			if (hook != null)
			{
				var success = hook.Detach();

				if (!success)
				{
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}

				SystemHooks.TryRemove(hookId, out _);
			}
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

		public (int x, int y) GetCursorPosition()
		{
			var position = new POINT();

			User32.GetCursorPos(ref position);

			return (position.X, position.Y);
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

		public IntPtr GetWindowIcon(IntPtr window)
		{
			var icon = User32.SendMessage(window, Constant.WM_GETICON, new IntPtr(Constant.ICON_BIG), IntPtr.Zero);

			if (icon == IntPtr.Zero)
			{
				icon = User32.SendMessage(window, Constant.WM_GETICON, new IntPtr(Constant.ICON_SMALL), IntPtr.Zero);
			}

			if (icon == IntPtr.Zero)
			{
				icon = User32.SendMessage(window, Constant.WM_GETICON, new IntPtr(Constant.ICON_SMALL2), IntPtr.Zero);
			}

			return icon;
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

		public bool HasInternetConnection()
		{
			return WinInet.InternetGetConnectedState(out _, 0);
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

		public Guid RegisterKeyboardHook(KeyboardHookCallback callback)
		{
			var hookId = default(Guid);
			var hookReadyEvent = new AutoResetEvent(false);
			var hookThread = new Thread(() =>
			{
				var hook = new KeyboardHook(callback);
				var sleepEvent = new AutoResetEvent(false);

				hook.Attach();
				hookId = hook.Id;
				KeyboardHooks[hookId] = hook;
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

			return hookId;
		}

		public Guid RegisterMouseHook(MouseHookCallback callback)
		{
			var hookId = default(Guid);
			var hookReadyEvent = new AutoResetEvent(false);
			var hookThread = new Thread(() =>
			{
				var hook = new MouseHook(callback);
				var sleepEvent = new AutoResetEvent(false);

				hook.Attach();
				hookId = hook.Id;
				MouseHooks[hookId] = hook;
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

			return hookId;
		}

		public Guid RegisterSystemCaptureStartEvent(SystemEventCallback callback)
		{
			return RegisterSystemEvent(callback, Constant.EVENT_SYSTEM_CAPTURESTART);
		}

		public Guid RegisterSystemForegroundEvent(SystemEventCallback callback)
		{
			return RegisterSystemEvent(callback, Constant.EVENT_SYSTEM_FOREGROUND);
		}

		internal Guid RegisterSystemEvent(SystemEventCallback callback, uint eventId)
		{
			var hookId = default(Guid);
			var hookReadyEvent = new AutoResetEvent(false);
			var hookThread = new Thread(() =>
			{
				var hook = new SystemHook(callback, eventId);

				hook.Attach();
				hookId = hook.Id;
				SystemHooks[hookId] = hook;
				hookReadyEvent.Set();
				hook.AwaitDetach();
			});

			hookThread.SetApartmentState(ApartmentState.STA);
			hookThread.IsBackground = true;
			hookThread.Start();

			hookReadyEvent.WaitOne();

			return hookId;
		}

		public void RemoveWallpaper()
		{
			SetWallpaper(string.Empty);
		}

		public void RestoreWindow(IntPtr window)
		{
			User32.ShowWindow(window, (int) ShowWindowCommand.Restore);
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
			var success = User32.SystemParametersInfo(SPI.SETDESKWALLPAPER, 0, filePath, SPIF.NONE);

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
