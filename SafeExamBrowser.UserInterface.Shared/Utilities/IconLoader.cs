/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SafeExamBrowser.UserInterface.Shared.Utilities
{
	public static class IconLoader
	{
		private const int MAX_PATH_CHARS = 260;
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
			try
			{
				using (var icon = Icon.ExtractAssociatedIcon(file.FullName))
				{
					return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
				}
			}
			catch
			{
				// Icon.ExtractAssociatedIcon(...) has proven to be error-prone, especially for network paths. Thus we try to use the native API
				// in those cases, see also https://stackoverflow.com/questions/1842226/how-to-get-the-associated-icon-from-a-network-share-file.
				var builder = new StringBuilder(MAX_PATH_CHARS);

				builder.Append(file.FullName);

				var handle = ExtractAssociatedIcon(IntPtr.Zero, builder, out _);

				using (var icon = Icon.FromHandle(handle))
				{
					return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
				}
			}
		}

		[DllImport("user32.dll")]
		private static extern int DestroyIcon(IntPtr hIcon);

		[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
		static extern IntPtr ExtractAssociatedIcon(IntPtr hInst, StringBuilder lpIconPath, out ushort lpiIcon);

		[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
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
