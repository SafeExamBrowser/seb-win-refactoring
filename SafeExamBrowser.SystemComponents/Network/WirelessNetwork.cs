/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.SystemComponents.Contracts.Network;
using Windows.Devices.WiFi;

namespace SafeExamBrowser.SystemComponents.Network
{
	internal class WirelessNetwork : IWirelessNetwork
	{
		internal WiFiAvailableNetwork Network { get; set; }

		public string Name { get; set; }
		public int SignalStrength { get; set; }
		public ConnectionStatus Status { get; set; }
	}
}
