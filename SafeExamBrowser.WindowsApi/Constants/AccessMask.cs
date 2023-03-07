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
	/// See https://docs.microsoft.com/de-de/windows/desktop/SecAuthZ/access-mask
	/// </remarks>
	[Flags]
	internal enum AccessMask : uint
	{
		DESKTOP_NONE = 0,
		DESKTOP_READOBJECTS = 0x0001,
		DESKTOP_CREATEWINDOW = 0x0002,
		DESKTOP_CREATEMENU = 0x0004,
		DESKTOP_HOOKCONTROL = 0x0008,
		DESKTOP_JOURNALRECORD = 0x0010,
		DESKTOP_JOURNALPLAYBACK = 0x0020,
		DESKTOP_ENUMERATE = 0x0040,
		DESKTOP_WRITEOBJECTS = 0x0080,
		DESKTOP_SWITCHDESKTOP = 0x0100,

		GENERIC_ALL = (DESKTOP_READOBJECTS | DESKTOP_CREATEWINDOW | DESKTOP_CREATEMENU | DESKTOP_HOOKCONTROL | DESKTOP_JOURNALRECORD
			| DESKTOP_JOURNALPLAYBACK | DESKTOP_ENUMERATE | DESKTOP_WRITEOBJECTS | DESKTOP_SWITCHDESKTOP
			| Constant.STANDARD_RIGHTS_REQUIRED)
	}
}
