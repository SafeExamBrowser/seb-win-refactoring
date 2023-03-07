/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Settings.Browser
{
	/// <summary>
	/// Defines all policies for browser window popups.
	/// </summary>
	public enum PopupPolicy
	{
		/// <summary>
		/// Allows all popups.
		/// </summary>
		Allow,

		/// <summary>
		/// Allows only popups which target the same host as the window from which they originate.
		/// </summary>
		AllowSameHost,

		/// <summary>
		/// Allows only popups which target the same host as the window from which they originate and opens every request directly in the respective window.
		/// </summary>
		AllowSameHostAndWindow,

		/// <summary>
		/// Allows all popups but opens every request directly in the window from which it originates.
		/// </summary>
		AllowSameWindow,

		/// <summary>
		/// Blocks all popups.
		/// </summary>
		Block
	}
}
