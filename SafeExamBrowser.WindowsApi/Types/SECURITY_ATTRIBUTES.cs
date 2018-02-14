/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.InteropServices;

namespace SafeExamBrowser.WindowsApi.Types
{
	/// <remarks>
	/// See http://pinvoke.net/default.aspx/Structures/SECURITY_ATTRIBUTES.html.
	/// See https://msdn.microsoft.com/en-us/library/windows/desktop/aa379560(v=vs.85).aspx.
	/// </remarks>
	[StructLayout(LayoutKind.Sequential)]
	internal struct SECURITY_ATTRIBUTES
	{
		public int nLength;
		public IntPtr lpSecurityDescriptor;
		public int bInheritHandle;
	}
}
