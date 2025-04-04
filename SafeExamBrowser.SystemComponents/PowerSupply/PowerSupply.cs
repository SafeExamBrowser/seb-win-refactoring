﻿/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Timers;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings.SystemComponents;
using SafeExamBrowser.SystemComponents.Contracts.PowerSupply;
using SafeExamBrowser.SystemComponents.Contracts.PowerSupply.Events;
using PowerLineStatus = System.Windows.Forms.PowerLineStatus;
using SystemInformation = System.Windows.Forms.SystemInformation;

namespace SafeExamBrowser.SystemComponents.PowerSupply
{
	public class PowerSupply : IPowerSupply
	{
		private readonly ILogger logger;
		private readonly PowerSupplySettings settings;

		private double critical;
		private double low;
		private DateTime lastStatusLog;
		private Timer timer;

		public event StatusChangedEventHandler StatusChanged;

		public PowerSupply(ILogger logger, PowerSupplySettings settings)
		{
			this.logger = logger;
			this.settings = settings;
		}

		public IPowerSupplyStatus GetStatus()
		{
			var charge = SystemInformation.PowerStatus.BatteryLifePercent;
			var hours = SystemInformation.PowerStatus.BatteryLifeRemaining / 3600;
			var minutes = (SystemInformation.PowerStatus.BatteryLifeRemaining - (hours * 3600)) / 60;
			var status = new PowerSupplyStatus();

			status.BatteryCharge = charge;
			status.BatteryChargeStatus = charge <= low ? (charge <= critical ? BatteryChargeStatus.Critical : BatteryChargeStatus.Low) : BatteryChargeStatus.Okay;
			status.BatteryTimeRemaining = new TimeSpan(hours, minutes, 0);
			status.IsOnline = SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online;

			if (lastStatusLog < DateTime.Now.AddMinutes(-1))
			{
				logger.Debug($"Power grid is {(status.IsOnline ? "" : "not ")}connected, battery charge at {charge * 100}%{(status.IsOnline ? "" : $" ({status.BatteryTimeRemaining})")}.");
				lastStatusLog = DateTime.Now;
			}

			return status;
		}

		public void Initialize()
		{
			const int FIVE_SECONDS = 5000;

			critical = SanitizeThreshold(settings.ChargeThresholdCritical);
			low = SanitizeThreshold(settings.ChargeThresholdLow);

			timer = new Timer(FIVE_SECONDS);
			timer.Elapsed += Timer_Elapsed;
			timer.AutoReset = true;
			timer.Start();

			logger.Info($"Started monitoring the power supply (battery charge thresholds: low = {low * 100}%, critical = {critical * 100}%).");
		}

		public void Terminate()
		{
			if (timer != null)
			{
				timer.Stop();
				logger.Info("Stopped monitoring the power supply.");
			}
		}

		private double SanitizeThreshold(double value)
		{
			return value < 0 ? 0 : (value > 1 ? 1 : value);
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			StatusChanged?.Invoke(GetStatus());
		}
	}
}
