/*
 * Copyright (c) 2026 ETH Zürich, IT Services
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
		/// <summary>
		/// This API has proven to be at least partially unreliably with respect to an actual connection to the internet,
		/// see also https://learn.microsoft.com/en-us/windows/win32/api/wininet/nf-wininet-internetgetconnectedstate.
		///
		/// For determination of an actual connection to the internet consider using the Windows Runtime API,
		/// see https://learn.microsoft.com/en-us/uwp/api/windows.networking.connectivity.networkinformation.
		/// </summary>
		[DllImport("wininet.dll", SetLastError = true)]
		internal static extern bool InternetGetConnectedState(out int description, int reservedValue);
	}
}
