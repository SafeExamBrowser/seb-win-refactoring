/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.SystemComponents;
using SimpleWifi;

namespace SafeExamBrowser.SystemComponents
{
	internal class WirelessNetworkDefinition : IWirelessNetwork
	{
		internal AccessPoint AccessPoint { get; set; }

		public Guid Id { get; }
		public string Name { get; set; }
		public int SignalStrength { get; set; }
		public WirelessNetworkStatus Status { get; set; }

		public WirelessNetworkDefinition()
		{
			Id = Guid.NewGuid();
		}
	}
}
