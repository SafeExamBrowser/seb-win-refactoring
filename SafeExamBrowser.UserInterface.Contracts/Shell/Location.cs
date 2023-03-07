/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.UserInterface.Contracts.Shell
{
	/// <summary>
	/// Defines all possible locations of a user control in the shell.
	/// </summary>
	public enum Location
	{
		/// <summary>
		/// A user control styled for and placed in the action center.
		/// </summary>
		ActionCenter,

		/// <summary>
		/// A user control styled for and placed in the taskbar.
		/// </summary>
		Taskbar
	}
}
