/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Settings.Security
{
	/// <summary>
	/// Defines all kiosk modes which SEB supports.
	/// </summary>
	public enum KioskMode
	{
		/// <summary>
		/// No kiosk mode - should only be used for testing / debugging.
		/// </summary>
		None,

		/// <summary>
		/// Creates a new desktop and runs the client on it, without modifying the default desktop.
		/// </summary>
		CreateNewDesktop,

		/// <summary>
		/// Terminates the Windows explorer shell and runs the client on the default desktop.
		/// </summary>
		DisableExplorerShell
	}
}
