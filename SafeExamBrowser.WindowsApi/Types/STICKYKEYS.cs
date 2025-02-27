/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Runtime.InteropServices;
using SafeExamBrowser.WindowsApi.Constants;

namespace SafeExamBrowser.WindowsApi.Types
{
	/// <remarks>
	/// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-stickykeys.
	/// </remarks>
	[StructLayout(LayoutKind.Sequential)]
	internal struct STICKYKEYS
	{
		internal int cbSize;
		internal StickyKeysFlags dwFlags;
	}
}
