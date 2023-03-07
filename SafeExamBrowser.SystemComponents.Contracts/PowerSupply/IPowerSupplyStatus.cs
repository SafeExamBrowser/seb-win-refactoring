/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.SystemComponents.Contracts.PowerSupply
{
	/// <summary>
	/// Provides data about the power supply of the system.
	/// </summary>
	public interface IPowerSupplyStatus
	{
		/// <summary>
		/// Defines the current charge of the system battery: <c>0.0</c> means the battery is empty, <c>1.0</c> means it's fully charged.
		/// </summary>
		double BatteryCharge { get; }

		/// <summary>
		/// Defines the current charge status of the system battery.
		/// </summary>
		BatteryChargeStatus BatteryChargeStatus { get; }

		/// <summary>
		/// Defines the time remaining until the battery is empty.
		/// </summary>
		TimeSpan BatteryTimeRemaining { get; }

		/// <summary>
		/// Indicates whether the system is connected to the power grid.
		/// </summary>
		bool IsOnline { get; }
	}
}
