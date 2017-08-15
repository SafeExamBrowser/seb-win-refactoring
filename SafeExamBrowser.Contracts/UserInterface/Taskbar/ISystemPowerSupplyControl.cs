/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.UserInterface.Taskbar
{
	public interface ISystemPowerSupplyControl : ISystemControl
	{
		/// <summary>
		/// Sets the current charge of the system battery, if available. <c>0.0</c> means the battery is empty, <c>1.0</c> means it's
		/// fully charged. Pass <c>null</c> to indicate that the computer system has no battery.
		/// </summary>
		void SetBatteryCharge(double? percentage);

		/// <summary>
		/// Sets the power supply status, i.e. whether the computer system is connected to the power grid or not.
		/// </summary>
		void SetPowerGridConnection(bool connected);
	}
}
