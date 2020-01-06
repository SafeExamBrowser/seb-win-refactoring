/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.SystemComponents.Contracts.WirelessNetwork;
using SimpleWifi;

namespace SafeExamBrowser.SystemComponents.WirelessNetwork
{
	internal class WirelessNetwork : IWirelessNetwork
	{
		internal AccessPoint AccessPoint { get; set; }

		public Guid Id { get; }
		public string Name { get; set; }
		public int SignalStrength { get; set; }
		public WirelessNetworkStatus Status { get; set; }

		public WirelessNetwork()
		{
			Id = Guid.NewGuid();
		}
	}
}
