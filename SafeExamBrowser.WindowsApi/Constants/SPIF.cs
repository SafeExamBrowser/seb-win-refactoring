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
	/// See http://www.pinvoke.net/default.aspx/Enums/SPIF.html.
	/// </remarks>
	[Flags]
	internal enum SPIF
	{
		NONE = 0x00,

		/// <summary>
		/// Writes the new system-wide parameter setting to the user profile.
		/// </summary>
		UPDATEINIFILE = 0x01,

		/// <summary>
		/// Broadcasts the WM_SETTINGCHANGE message after updating the user profile.
		/// </summary>
		SENDCHANGE = 0x02,

		/// <summary>
		/// Performs UPDATEINIFILE and SENDCHANGE.
		/// </summary>
		UPDATEANDCHANGE = 0x03
	}
}
