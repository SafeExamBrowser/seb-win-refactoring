/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.WindowsApi.Types
{
	/// <remarks>
	/// See http://www.pinvoke.net/default.aspx/kernel32/SetThreadExecutionState.html.
	/// See https://msdn.microsoft.com/en-us/library/aa373208(v=vs.85).aspx.
	/// </remarks>
	[Flags]
	public enum EXECUTION_STATE : uint
	{
		AWAYMODE_REQUIRED = 0x00000040,
		CONTINUOUS = 0x80000000,
		DISPLAY_REQUIRED = 0x00000002,
		SYSTEM_REQUIRED = 0x00000001
	}
}
