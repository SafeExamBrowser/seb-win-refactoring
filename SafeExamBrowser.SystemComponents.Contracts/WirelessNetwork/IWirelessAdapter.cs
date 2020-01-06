/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.SystemComponents.Contracts.WirelessNetwork.Events;

namespace SafeExamBrowser.SystemComponents.Contracts.WirelessNetwork
{
	public interface IWirelessAdapter : ISystemComponent
	{
		/// <summary>
		/// Fired when the available wireless networks changed.
		/// </summary>
		event NetworksChangedEventHandler NetworksChanged;

		/// <summary>
		/// Fired when the wireless network status changed.
		/// </summary>
		event StatusChangedEventHandler StatusChanged;

		/// <summary>
		/// Indicates whether the system has an active wireless network adapter.
		/// </summary>
		bool IsAvailable { get; }

		/// <summary>
		/// Attempts to connect to the wireless network with the given ID.
		/// </summary>
		void Connect(Guid id);

		/// <summary>
		/// Retrieves all currently available networks.
		/// </summary>
		IEnumerable<IWirelessNetwork> GetNetworks();
	}
}
