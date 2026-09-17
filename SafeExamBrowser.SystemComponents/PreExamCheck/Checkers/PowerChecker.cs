/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows.Forms;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.PreExamCheck;

namespace SafeExamBrowser.SystemComponents.PreExamCheck.Checkers
{
	public class PowerChecker : IChecker
	{
		private readonly ISystemInfo systemInfo;

		public string Name => "Battery Status / Power Source";

		public PowerChecker(ISystemInfo systemInfo)
		{
			this.systemInfo = systemInfo;
		}

		public CheckResult Check()
		{
			var status = SystemInformation.PowerStatus;
			bool hasBattery = systemInfo.HasBattery;
			float batteryPercent = status.BatteryLifePercent * 100;
			bool isCharging = status.PowerLineStatus == PowerLineStatus.Online;

			if (!hasBattery)
			{
				return new CheckResult
				{
					Category = "Power",
					Title = Name,
					Status = CheckStatus.Passed,
					ActualValue = "Desktop (PC)",
					RequiredValue = "Plugged into AC / Battery > 20%",
					Message = "Desktop (PC) is using direct power source.",
					IsCritical = false
				};
			}

			bool passed = isCharging || batteryPercent >= 20;
			CheckStatus resultStatus = passed ? (isCharging ? CheckStatus.Passed : CheckStatus.Warning) : CheckStatus.Failed;

			string actualText = isCharging ? $"Charging ({batteryPercent:F0}%)" : $"Using Battery ({batteryPercent:F0}%)";

			return new CheckResult
			{
				Category = "Power",
				Title = Name,
				Status = resultStatus,
				ActualValue = actualText,
				RequiredValue = "Charging or Battery ≥ 20%",
				Message = isCharging 
					? "Device is plugged in and charging." 
					: (batteryPercent >= 20 ? "You should plug in your device to charge the battery before starting the exam." : "Battery level is too low, please connect to a power source."),
				IsCritical = false
			};
		}
	}
}
