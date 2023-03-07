/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.SystemComponents.Contracts.PowerSupply.Events;

namespace SafeExamBrowser.SystemComponents.Contracts.PowerSupply
{
	/// <summary>
	/// Defines the functionality of the power supply.
	/// </summary>
	public interface IPowerSupply : ISystemComponent
	{
		/// <summary>
		/// Fired when the status of the power supply changed.
		/// </summary>
		event StatusChangedEventHandler StatusChanged;

		/// <summary>
		/// Retrieves the current status of the power supply.
		/// </summary>
		IPowerSupplyStatus GetStatus();
	}
}
