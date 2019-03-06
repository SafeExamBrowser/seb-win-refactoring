/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Timers;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.SystemComponents;
using SafeExamBrowser.Contracts.UserInterface.Shell;
using PowerLineStatus = System.Windows.Forms.PowerLineStatus;
using SystemInformation = System.Windows.Forms.SystemInformation;

namespace SafeExamBrowser.SystemComponents
{
	public class PowerSupply : ISystemComponent<ISystemPowerSupplyControl>
	{
		private const int TWO_SECONDS = 2000;

		private bool infoShown, warningShown;
		private ILogger logger;
		private ISystemPowerSupplyControl control;
		private IText text;
		private Timer timer;

		public PowerSupply(ILogger logger, IText text)
		{
			this.logger = logger;
			this.text = text;
		}

		public void Initialize(ISystemPowerSupplyControl control)
		{
			this.control = control;

			UpdateControl();

			timer = new Timer(TWO_SECONDS);
			timer.Elapsed += Timer_Elapsed;
			timer.AutoReset = true;
			timer.Start();

			logger.Info("Started monitoring the power supply.");
		}

		public void Terminate()
		{
			timer?.Stop();
			control?.Close();
			logger.Info("Stopped monitoring the power supply.");
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			UpdateControl();
		}

		private void UpdateControl()
		{
			var charge = SystemInformation.PowerStatus.BatteryLifePercent;
			var percentage = Math.Round(charge * 100);
			var status = charge <= 0.4 ? (charge <= 0.2 ? BatteryChargeStatus.Critical : BatteryChargeStatus.Low) : BatteryChargeStatus.Okay;
			var online = SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online;
			var tooltip = string.Empty;

			if (online)
			{
				tooltip = text.Get(percentage == 100 ? TextKey.SystemControl_BatteryCharged : TextKey.SystemControl_BatteryCharging);
				infoShown = false;
				warningShown = false;
			}
			else
			{
				var hours = SystemInformation.PowerStatus.BatteryLifeRemaining / 3600;
				var minutes = (SystemInformation.PowerStatus.BatteryLifeRemaining - (hours * 3600)) / 60;

				HandleBatteryStatus(status);

				tooltip = text.Get(TextKey.SystemControl_BatteryRemainingCharge);
				tooltip = tooltip.Replace("%%HOURS%%", hours.ToString());
				tooltip = tooltip.Replace("%%MINUTES%%", minutes.ToString());
			}

			tooltip = tooltip.Replace("%%CHARGE%%", percentage.ToString());

			control.SetBatteryCharge(charge, status);
			control.SetPowerGridConnection(online);
			control.SetTooltip(tooltip);
		}

		private void HandleBatteryStatus(BatteryChargeStatus status)
		{
			if (status == BatteryChargeStatus.Low && !infoShown)
			{
				control.ShowLowBatteryInfo(text.Get(TextKey.SystemControl_BatteryChargeLowInfo));
				infoShown = true;
				logger.Info("Informed the user about low battery charge.");
			}

			if (status == BatteryChargeStatus.Critical && !warningShown)
			{
				control.ShowCriticalBatteryWarning(text.Get(TextKey.SystemControl_BatteryChargeCriticalWarning));
				warningShown = true;
				logger.Warn("Warned the user about critical battery charge.");
			}
		}
	}
}
