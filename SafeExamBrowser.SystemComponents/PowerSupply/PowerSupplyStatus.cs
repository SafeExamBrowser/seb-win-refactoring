/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.SystemComponents.Contracts.PowerSupply;

namespace SafeExamBrowser.SystemComponents.PowerSupply
{
	internal class PowerSupplyStatus : IPowerSupplyStatus
	{
		public double BatteryCharge { get; set; }
		public BatteryChargeStatus BatteryChargeStatus { get; set; }
		public TimeSpan BatteryTimeRemaining { get; set; }
		public bool IsOnline { get; set; }
	}
}
