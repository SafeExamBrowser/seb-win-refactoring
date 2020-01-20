/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SafeExamBrowser.UserInterface.Shared.Utilities
{
	public static class IconLoader
	{
		private const uint SHGFI_ICON = 0x100;
		private const uint SHGFI_LARGEICON = 0x0;
		private const uint SHGFI_SMALLICON = 0x1;

		public static ImageSource LoadIconFor(DirectoryInfo directory)
		{
			var fileInfo = new SHFILEINFO();
			var result = SHGetFileInfo(directory.FullName, 0, ref fileInfo, (uint) Marshal.SizeOf(fileInfo), SHGFI_ICON | SHGFI_SMALLICON);
			var imageSource = Imaging.CreateBitmapSourceFromHIcon(fileInfo.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

			DestroyIcon(fileInfo.hIcon);

			return imageSource;
		}

		public static ImageSource LoadIconFor(FileInfo file)
		{
			using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(file.FullName))
			{
				return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
			}
		}

		[DllImport("user32.dll")]
		private static extern int DestroyIcon(IntPtr hIcon);

		[DllImport("shell32.dll")]
		private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

		[StructLayout(LayoutKind.Sequential)]
		private struct SHFILEINFO
		{
			public IntPtr hIcon;
			public IntPtr iIcon;
			public uint dwAttributes;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szDisplayName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
			public string szTypeName;
		};
	}
}
