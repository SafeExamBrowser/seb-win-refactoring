/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.WindowsApi.Constants
{
	/// <remarks>
	/// See https://docs.microsoft.com/en-us/windows/desktop/inputdev/virtual-key-codes.
	/// </remarks>
	internal enum VirtualKeyCode
	{
		A = 0x41,
		Q = 0x51,
		Delete = 0x2E,
		LeftAlt = 0xA4,
		LeftControl = 0xA2,
		LeftWindows = 0x5B,
		RightAlt = 0xA5,
		RightControl = 0xA3
	}
}
