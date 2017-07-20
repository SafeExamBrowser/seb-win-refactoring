/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.WindowsApi.Constants
{
	/// <remarks>
	/// See http://www.pinvoke.net/default.aspx/Enums/SPI.html?diff=y.
	/// </remarks>
	internal enum SPI : uint
	{
		/// <summary>
		/// Sets the size of the work area. The work area is the portion of the screen not obscured by the system taskbar
		/// or by application desktop toolbars. The pvParam parameter is a pointer to a RECT structure that specifies the
		/// new work area rectangle, expressed in virtual screen coordinates. In a system with multiple display monitors,
		/// the function sets the work area of the monitor that contains the specified rectangle.
		/// </summary>
		SETWORKAREA = 0x002F,

		/// <summary>
		/// Retrieves the size of the work area on the primary display monitor. The work area is the portion of the screen
		/// not obscured by the system taskbar or by application desktop toolbars. The pvParam parameter must point to a
		/// RECT structure that receives the coordinates of the work area, expressed in virtual screen coordinates. To get
		/// the work area of a monitor other than the primary display monitor, call the GetMonitorInfo function.
		/// </summary>
		GETWORKAREA = 0x0030,
	}
}
