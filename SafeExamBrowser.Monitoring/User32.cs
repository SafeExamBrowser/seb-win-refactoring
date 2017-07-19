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

namespace SafeExamBrowser.Monitoring
{
	/// <summary>
	/// Provides access to the native Windows API exposed by <c>user32.dll</c>.
	/// </summary>
	internal static class User32
	{
		internal static IntPtr GetShellWindowHandle()
		{
			return FindWindow("Shell_TrayWnd", null);
		}

		internal static uint GetShellProcessId()
		{
			var handle = FindWindow("Shell_TrayWnd", null);
			var threadId = GetWindowThreadProcessId(handle, out uint processId);

			return processId;
		}

		/// <remarks>
		/// The close message <c>0x5B4</c> posted to the shell is undocumented and not officially supported:
		/// https://stackoverflow.com/questions/5689904/gracefully-exit-explorer-programmatically/5705965#5705965
		/// </remarks>
		internal static void PostCloseMessageToShell()
		{
			var taskbarHandle = FindWindow("Shell_TrayWnd", null);
			var success = PostMessage(taskbarHandle, 0x5B4, IntPtr.Zero, IntPtr.Zero);

			if (!success)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
		}

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
	}
}
