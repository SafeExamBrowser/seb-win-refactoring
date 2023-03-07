/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.SystemComponents.Contracts.Network.Events;

namespace SafeExamBrowser.SystemComponents.Contracts.Network
{
	/// <summary>
	/// Defines the functionality of the network adapter system component.
	/// </summary>
	public interface INetworkAdapter : ISystemComponent
	{
		/// <summary>
		/// The connection status of the network adapter.
		/// </summary>
		ConnectionStatus Status { get; }

		/// <summary>
		/// The type of the current network connection.
		/// </summary>
		ConnectionType Type { get; }

		/// <summary>
		/// Fired when the network adapter has changed.
		/// </summary>
		event ChangedEventHandler Changed;

		/// <summary>
		/// Attempts to connect to the wireless network with the given ID.
		/// </summary>
		void ConnectToWirelessNetwork(Guid id);

		/// <summary>
		/// Retrieves all currently available wireless networks.
		/// </summary>
		IEnumerable<IWirelessNetwork> GetWirelessNetworks();
	}
}
