/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.SystemComponents.Contracts.PowerSupply
{
	/// <summary>
	/// Defines all possible battery charge statuses which can be determined by the application.
	/// </summary>
	public enum BatteryChargeStatus
	{
		Undefined = 0,

		/// <summary>
		/// The battery charge is critical, i.e. below 20%.
		/// </summary>
		Critical,

		/// <summary>
		/// The battery charge is low, i.e. below 35%.
		/// </summary>
		Low,

		/// <summary>
		/// The battery charge is okay, i.e. above 35%.
		/// </summary>
		Okay
	}
}
