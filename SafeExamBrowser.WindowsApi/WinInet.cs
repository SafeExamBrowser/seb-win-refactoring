/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Runtime.InteropServices;

namespace SafeExamBrowser.WindowsApi
{
	/// <summary>
	/// Provides access to the native Windows API exposed by <c>wininet.dll</c>.
	/// </summary>
	internal static class WinInet
	{
		[DllImport("wininet.dll", SetLastError = true)]
		internal static extern bool InternetGetConnectedState(out int description, int reservedValue);
	}
}
