/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.SystemComponents;

namespace SafeExamBrowser.Contracts.UserInterface.Taskbar
{
	/// <summary>
	/// The control of the power supply system component.
	/// </summary>
	public interface ISystemPowerSupplyControl : ISystemControl
	{
		/// <summary>
		/// Sets the current charge of the system battery: <c>0.0</c> means the battery is empty, <c>1.0</c> means it's fully charged.
		/// </summary>
		void SetBatteryCharge(double charge, BatteryChargeStatus status);

		/// <summary>
		/// Sets the power supply status, i.e. whether the computer system is connected to the power grid or not.
		/// </summary>
		void SetPowerGridConnection(bool connected);

		/// <summary>
		/// Warns the user that the battery charge is critical.
		/// </summary>
		void ShowCriticalBatteryWarning(string warning);

		/// <summary>
		/// Indicates the user that the battery charge is low.
		/// </summary>
		void ShowLowBatteryInfo(string info);
	}
}
