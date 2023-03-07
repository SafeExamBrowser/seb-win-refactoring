/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Settings.Proctoring
{
	/// <summary>
	/// Defines all possible visibility states for the proctoring window.
	/// </summary>
	public enum WindowVisibility
	{
		/// <summary>
		/// The proctoring window is hidden and cannot be shown by the user.
		/// </summary>
		Hidden,

		/// <summary>
		/// The proctoring window is initially hidden but may be shown by the user.
		/// </summary>
		AllowToShow,

		/// <summary>
		/// The proctoring window is initially visible but may be hidden by the user.
		/// </summary>
		AllowToHide,

		/// <summary>
		/// The proctoring window is always visible and cannot be hidden by the user.
		/// </summary>
		Visible
	}
}
