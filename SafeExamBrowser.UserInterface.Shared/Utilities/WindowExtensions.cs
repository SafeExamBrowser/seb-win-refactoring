/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace SafeExamBrowser.UserInterface.Shared.Utilities
{
	public static class WindowExtensions
	{
		private const int GWL_STYLE = -16;
		private const uint MF_BYCOMMAND = 0x00000000;
		private const uint MF_GRAYED = 0x00000001;
		private const uint MF_ENABLED = 0x00000000;
		private const uint SC_CLOSE = 0xF060;
		private const uint SWP_SHOWWINDOW = 0x0040;
		private const int WS_SYSMENU = 0x80000;

		private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

		public static void DisableCloseButton(this Window window)
		{
			var helper = new WindowInteropHelper(window);
			var systemMenu = GetSystemMenu(helper.Handle, false);

			if (systemMenu != IntPtr.Zero)
			{
				EnableMenuItem(systemMenu, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
			}
		}

		public static void HideCloseButton(this Window window)
		{
			var helper = new WindowInteropHelper(window);
			var style = GetWindowLong(helper.Handle, GWL_STYLE) & ~WS_SYSMENU;

			SetWindowLong(helper.Handle, GWL_STYLE, style);
		}

		public static void MoveToBackground(this Window window)
		{
			var helper = new WindowInteropHelper(window);
			var x = (int) window.TransformFromPhysical(window.Left, 0).X;
			var y = (int) window.TransformFromPhysical(0, window.Top).Y;
			var width = (int) window.TransformFromPhysical(window.Width, 0).X;
			var height = (int) window.TransformFromPhysical(0, window.Height).Y;

			SetWindowPos(helper.Handle, HWND_BOTTOM, x, y, width, height, SWP_SHOWWINDOW);
		}

		[DllImport("user32.dll")]
		private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

		[DllImport("user32.dll")]
		private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

		[DllImport("user32.dll")]
		private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll")]
		private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		[DllImport("user32.dll")]
		private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
	}
}
