/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Contracts.SystemComponents
{
	public interface IWirelessNetwork
	{
		/// <summary>
		/// The unique identifier of the network.
		/// </summary>
		Guid Id { get; }

		/// <summary>
		/// The network name.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// The signal strength of this network, as percentage.
		/// </summary>
		int SignalStrength { get; }

		/// <summary>
		/// The connection status of this network.
		/// </summary>
		WirelessNetworkStatus Status { get; }
	}
}
