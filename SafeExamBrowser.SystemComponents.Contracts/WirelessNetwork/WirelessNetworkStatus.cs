/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.SystemComponents.Contracts.WirelessNetwork
{
	/// <summary>
	/// Defines all possible wireless network statuses which can be determined by the application.
	/// </summary>
	public enum WirelessNetworkStatus
	{
		Undefined = 0,

		/// <summary>
		/// A connection has been established.
		/// </summary>
		Connected,

		/// <summary>
		/// A connection is being established.
		/// </summary>
		Connecting,

		/// <summary>
		/// No connection is established.
		/// </summary>
		Disconnected
	}
}
