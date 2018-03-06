/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Contracts.SystemComponents;

namespace SafeExamBrowser.Contracts.UserInterface.Taskbar
{
	public delegate void WirelessNetworkSelectedEventHandler(IWirelessNetwork network);

	/// <summary>
	/// The control of the wireless network system component.
	/// </summary>
	public interface ISystemWirelessNetworkControl : ISystemControl
	{
		/// <summary>
		/// Defines whether the computer has a wireless network adapter.
		/// </summary>
		bool HasWirelessNetworkAdapter { set; }

		/// <summary>
		/// Indicates to the user that a wireless network connection is being established.
		/// </summary>
		bool IsConnecting { set; }

		/// <summary>
		/// Sets the current wireless network status.
		/// </summary>
		WirelessNetworkStatus NetworkStatus { set; }

		/// <summary>
		/// Event fired when the user selected a wireless network.
		/// </summary>
		event WirelessNetworkSelectedEventHandler NetworkSelected;

		/// <summary>
		/// Updates the list of available networks.
		/// </summary>
		void Update(IEnumerable<IWirelessNetwork> networks);
	}
}
