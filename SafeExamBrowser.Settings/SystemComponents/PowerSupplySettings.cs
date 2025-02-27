/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Settings.SystemComponents
{
	/// <summary>
	/// Defines all settings for the power supply system component.
	/// </summary>
	[Serializable]
	public class PowerSupplySettings
	{
		/// <summary>
		/// The threshold below which the charge of the power supply is to be considered critical.
		/// </summary>
		public double ChargeThresholdCritical { get; set; }

		/// <summary>
		/// The threshold below which the charge of the power supply is to be considered low.
		/// </summary>
		public double ChargeThresholdLow { get; set; }
	}
}
