/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SafeExamBrowser.WindowsApi.Constants;
using SafeExamBrowser.WindowsApi.Types;

namespace SafeExamBrowser.WindowsApi
{
	/// <summary>
	/// Provides access to the native Windows API exposed by <c>user32.dll</c>.
	/// </summary>
	public static class User32
	{
		/// <summary>
		/// Retrieves a window handle to the Windows taskbar. Returns <c>IntPtr.Zero</c>
		/// if the taskbar could not be found (i.e. if it isn't running).
		/// </summary>
		public static IntPtr GetShellWindowHandle()
		{
			return FindWindow("Shell_TrayWnd", null);
		}

		/// <summary>
		/// Retrieves the process ID of the main Windows explorer instance controlling
		/// desktop and taskbar or <c>0</c>, if the process isn't running.
		/// </summary>
		/// <returns></returns>
		public static uint GetShellProcessId()
		{
			var handle = GetShellWindowHandle();
			var threadId = GetWindowThreadProcessId(handle, out uint processId);

			return processId;
		}

		/// <summary>
		/// Retrieves the currently configured working area of the primary screen.
		/// </summary>
		/// <exception cref="System.ComponentModel.Win32Exception">
		/// If the working area could not be retrieved.
		/// </exception>
		public static RECT GetWorkingArea()
		{
			var workingArea = new RECT();
			var success = SystemParametersInfo(SPI.GETWORKAREA, 0, ref workingArea, SPIF.NONE);

			if (!success)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}

			return workingArea;
		}

		/// <summary>
		/// Minimizes all open windows.
		/// </summary>
		public static void MinimizeAllOpenWindows()
		{
			var handle = GetShellWindowHandle();

			SendMessage(handle, Constant.WM_COMMAND, (IntPtr) Constant.MIN_ALL, IntPtr.Zero);
		}

		/// <summary>
		/// Instructs the main Windows explorer process to shut down.
		/// </summary>
		/// <exception cref="System.ComponentModel.Win32Exception">
		/// If the messge could not be successfully posted. Does not apply if the process isn't running!
		/// </exception>
		/// <remarks>
		/// The close message <c>0x5B4</c> posted to the shell is undocumented and not officially supported:
		/// https://stackoverflow.com/questions/5689904/gracefully-exit-explorer-programmatically/5705965#5705965
		/// </remarks>
		public static void PostCloseMessageToShell()
		{
			var handle = GetShellWindowHandle();
			var success = PostMessage(handle, 0x5B4, IntPtr.Zero, IntPtr.Zero);

			if (!success)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
		}

		/// <summary>
		/// Sets the working area of the primary screen according to the given dimensions.
		/// </summary>
		/// <exception cref="System.ComponentModel.Win32Exception">
		/// If the working area could not be set.
		/// </exception>
		public static void SetWorkingArea(RECT workingArea)
		{
			var success = SystemParametersInfo(SPI.SETWORKAREA, 0, ref workingArea, SPIF.UPDATEANDCHANGE);

			if (!success)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
		}

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", EntryPoint = "SendMessage")]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool SystemParametersInfo(SPI uiAction, uint uiParam, ref RECT pvParam, SPIF fWinIni);
	}
}
