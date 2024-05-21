/*
 * Copyright (c) 2024 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using SafeExamBrowser.SystemComponents.Contracts.Network;
using Windows.Devices.WiFi;

namespace SafeExamBrowser.SystemComponents.Network
{
	internal static class Extensions
	{
		internal static IOrderedEnumerable<WiFiAvailableNetwork> FilterAndOrder(this IReadOnlyList<WiFiAvailableNetwork> networks)
		{
			return networks.Where(n => !string.IsNullOrEmpty(n.Ssid)).GroupBy(n => n.Ssid).Select(g => g.First()).OrderBy(n => n.Ssid);
		}

		internal static WirelessNetwork ToWirelessNetwork(this WiFiAvailableNetwork network)
		{
			return new WirelessNetwork
			{
				Name = network.Ssid,
				Network = network,
				SignalStrength = Convert.ToInt32(Math.Max(0, Math.Min(100, (network.NetworkRssiInDecibelMilliwatts + 100) * 2))),
				Status = ConnectionStatus.Disconnected
			};
		}
	}
}
