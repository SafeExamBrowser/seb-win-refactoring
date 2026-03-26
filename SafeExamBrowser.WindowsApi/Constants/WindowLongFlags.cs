/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.WindowsApi.Constants
{
	/// <remarks>
	/// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowlonga.
	/// </remarks>
	internal enum WindowLongFlags : int
	{
		/// <summary>
		/// Retrieves the return value of a message processed in the dialog box procedure.
		/// </summary>
		DWL_MSGRESULT = 0x0,

		/// <summary>
		/// Retrieves the address of the dialog box procedure, or a handle representing the address of the dialog box procedure. You must use the
		/// CallWindowProc function to call the dialog box procedure. 
		/// </summary>
		DWL_DLGPROC = 0x4,

		/// <summary>
		/// Retrieves extra information private to the application, such as handles or pointers.
		/// </summary>
		DWL_USER = 0x8,

		/// <summary>
		/// Retrieves the extended window styles.
		/// </summary>
		GWL_EXSTYLE = -20,

		/// <summary>
		/// Retrieves a handle to the application instance.
		/// </summary>
		GWL_HINSTANCE = -6,

		/// <summary>
		/// Retrieves a handle to the parent window, if any.
		/// </summary>
		GWL_HWNDPARENT = -8,

		/// <summary>
		/// Retrieves the identifier of the window.
		/// </summary>
		GWL_ID = -12,

		/// <summary>
		/// Retrieves the window styles.
		/// </summary>
		GWL_STYLE = -16,

		/// <summary>
		/// Retrieves the user data associated with the window. This data is intended for use by the application that created the window. Its value
		/// is initially zero.
		/// </summary>
		GWL_USERDATA = -21,

		/// <summary>
		/// Retrieves the address of the window procedure, or a handle representing the address of the window procedure. You must use the
		/// CallWindowProc function to call the window procedure.
		/// </summary>
		GWL_WNDPROC = -4
	}
}
