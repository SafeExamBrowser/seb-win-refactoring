/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.SystemComponents.Contracts.Network
{
	/// <summary>
	/// Defines all possible connection types which can be determined by the application.
	/// </summary>
	public enum ConnectionType
	{
		/// <summary>
		/// The connection type cannot be determined.
		/// </summary>
		Undefined = 0,

		/// <summary>
		/// A wired network connection.
		/// </summary>
		Wired,

		/// <summary>
		/// A wireless network connection.
		/// </summary>
		Wireless
	}
}
