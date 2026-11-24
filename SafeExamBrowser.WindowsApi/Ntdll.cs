/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.InteropServices;

namespace SafeExamBrowser.WindowsApi
{
	internal class Ntdll
	{
		internal const int ThreadSuspendCount = 35;

		[DllImport("ntdll.dll")]
		internal static extern int NtQueryInformationThread(IntPtr thread, int informationClass, out uint information, int length, IntPtr returnLength);
	}
}
