/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
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
		/// Allows popups to be opened.
		/// </summary>
		Allow,

		/// <summary>
		/// Blocks all popups.
		/// </summary>
		Block,

		/// <summary>
		/// Opens popup requests in the same window from which they originate.
		/// </summary>
		SameWindow
	}
}
