/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.InteropServices;

namespace SafeExamBrowser.UserInterface.Shared.Utilities
{
	/// <remarks>
	/// See https://docs.microsoft.com/en-us/windows/win32/dwm/thumbnail-ovw.
	/// </remarks>
	public static class Thumbnail
	{
		public const int DWM_TNP_VISIBLE = 0x8;
		public const int DWM_TNP_OPACITY = 0x4;
		public const int DWM_TNP_RECTDESTINATION = 0x1;
		public const int S_OK = 0;

		[DllImport("dwmapi.dll")]
		public static extern int DwmQueryThumbnailSourceSize(IntPtr thumbnail, out Size size);

		[DllImport("dwmapi.dll")]
		public static extern int DwmRegisterThumbnail(IntPtr destinationWindow, IntPtr sourceWindow, out IntPtr thumbnail);

		[DllImport("dwmapi.dll")]
		public static extern int DwmUnregisterThumbnail(IntPtr thumbnail);

		[DllImport("dwmapi.dll")]
		public static extern int DwmUpdateThumbnailProperties(IntPtr thumbnail, ref Properties properties);

		[StructLayout(LayoutKind.Sequential)]
		public struct Properties
		{
			public int Flags;
			public Rectangle Destination;
			public Rectangle Source;
			public byte Opacity;
			public bool Visible;
			public bool SourceClientAreaOnly;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct Size
		{
			public int X;
			public int Y;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct Rectangle
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}
	}
}
