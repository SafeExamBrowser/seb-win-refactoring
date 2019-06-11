/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Configuration;

namespace SafeExamBrowser.Service
{
	/// <summary>
	/// Holds all configuration and runtime data required for the session handling.
	/// </summary>
	internal class SessionContext
	{
		/// <summary>
		/// The configuration of the currently active session.
		/// </summary>
		internal ServiceConfiguration Current { get; set; }

		/// <summary>
		/// The configuration of the next session to be activated.
		/// </summary>
		internal ServiceConfiguration Next { get; set; }
	}
}
