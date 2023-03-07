/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.WindowsApi.Constants
{
	/// <remarks>
	/// See https://docs.microsoft.com/en-us/windows/desktop/ProcThread/thread-security-and-access-rights.
	/// </remarks>
	[Flags]
	internal enum ThreadAccess : int
	{
		TERMINATE = 0x1,
		SUSPEND_RESUME = 0x2,
		GET_CONTEXT = 0x8,
		SET_CONTEXT = 0x10,
		SET_INFORMATION = 0x20,
		QUERY_INFORMATION = 0x40,
		SET_THREAD_TOKEN = 0x80,
		IMPERSONATE = 0x100,
		DIRECT_IMPERSONATION = 0x200
	}
}
