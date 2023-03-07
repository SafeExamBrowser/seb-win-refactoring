/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Settings
{
	/// <summary>
	/// Defines all possible session modes.
	/// </summary>
	public enum SessionMode
	{
		/// <summary>
		/// SEB will start a normal session without SEB server.
		/// </summary>
		Normal,

		/// <summary>
		/// SEB will start a session with SEB server.
		/// </summary>
		Server
	}
}
