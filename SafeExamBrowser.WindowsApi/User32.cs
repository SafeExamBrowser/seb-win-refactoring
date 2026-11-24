/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.InteropServices;
using System.Text;
using SafeExamBrowser.WindowsApi.Constants;
using SafeExamBrowser.WindowsApi.Delegates;
using SafeExamBrowser.WindowsApi.Types;

namespace SafeExamBrowser.WindowsApi
{
	/// <summary>
	/// Provides access to the native Windows API exposed by <c>user32.dll</c>.
	/// </summary>
	internal static class User32
	{
		// Fixed-width fields from DISPLAYCONFIG_PATH_INFO. Windows fills the entire
		// structure, but only the source identity and target technology are needed.
		[StructLayout(LayoutKind.Explicit, Size = 72)]
		internal struct DisplayConfigPath
		{
			[FieldOffset(0)] internal long SourceAdapter;
			[FieldOffset(8)] internal uint SourceId;
			[FieldOffset(36)] internal int OutputTechnology;
		}

		// DISPLAYCONFIG_MODE_INFO includes a native union. Its contents are unused.
		[StructLayout(LayoutKind.Sequential, Size = 64)]
		internal struct DisplayConfigMode { }

		[DllImport("user32.dll")]
		internal static extern int GetDisplayConfigBufferSizes(uint flags, out uint pathCount, out uint modeCount);

		[DllImport("user32.dll")]
		internal static extern int QueryDisplayConfig(uint flags, ref uint pathCount, [Out] DisplayConfigPath[] paths,
			ref uint modeCount, [Out] DisplayConfigMode[] modes, IntPtr topology);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern bool CloseClipboard();

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern bool CloseDesktop(IntPtr hDesktop);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern IntPtr CreateDesktop(string lpszDesktop, IntPtr lpszDevice, IntPtr pDevmode, int dwFlags, uint dwDesiredAccess, IntPtr lpsa);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern bool BringWindowToTop(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern bool EmptyClipboard();

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern bool EnumDesktops(IntPtr hwinsta, EnumDesktopDelegate lpEnumFunc, IntPtr lParam);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern bool EnumWindows(EnumWindowsDelegate enumProc, IntPtr lParam);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern bool GetCursorPos(ref POINT pt);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern IntPtr GetThreadDesktop(int dwThreadId);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern IntPtr GetProcessWindowStation();

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern bool GetUserObjectInformation(IntPtr hObj, int nIndex, IntPtr pvInfo, int nLength, ref int lpnLengthNeeded);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern uint GetWindowLong(IntPtr hWnd, WindowLongFlags nIndex);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern int GetWindowTextLength(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern bool IsIconic(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern bool IsWindow(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern bool IsWindowVisible(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern bool OpenClipboard(IntPtr hWndNewOwner);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern IntPtr OpenInputDesktop(uint dwFlags, bool fInherit, uint dwDesiredAccess);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, EventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern IntPtr SetWindowsHookEx(HookType hookType, HookDelegate lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern bool SwitchDesktop(IntPtr hDesktop);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern bool SystemParametersInfo(SPI uiAction, uint uiParam, ref RECT pvParam, SPIF fWinIni);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern bool SystemParametersInfo(SPI uiAction, int uiParam, ref STICKYKEYS pvParam, SPIF fWinIni);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern bool SystemParametersInfo(SPI uiAction, int uiParam, string pvParam, SPIF fWinIni);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern bool UnhookWinEvent(IntPtr hWinEventHook);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern bool UnhookWindowsHookEx(IntPtr hhk);
	}
}
