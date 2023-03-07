/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.PowerSupply;
using SafeExamBrowser.UserInterface.Contracts.Shell;

namespace SafeExamBrowser.UserInterface.Desktop.Controls.ActionCenter
{
	internal partial class PowerSupplyControl : UserControl, ISystemControl
	{
		private Brush initialBrush;
		private bool infoShown, warningShown;
		private double maxWidth;
		private IPowerSupply powerSupply;
		private IText text;

		internal PowerSupplyControl(IPowerSupply powerSupply, IText text)
		{
			this.powerSupply = powerSupply;
			this.text = text;

			InitializeComponent();
			InitializePowerSupplyControl();
		}

		public void Close()
		{
		}

		public void SetInformation(string text)
		{
			Dispatcher.InvokeAsync(() => Text.Text = text);
		}

		private void InitializePowerSupplyControl()
		{
			initialBrush = BatteryCharge.Fill;
			maxWidth = BatteryCharge.Width;
			powerSupply.StatusChanged += PowerSupply_StatusChanged;
			UpdateStatus(powerSupply.GetStatus());
		}

		private void PowerSupply_StatusChanged(IPowerSupplyStatus status)
		{
			Dispatcher.InvokeAsync(() => UpdateStatus(status));
		}

		private void UpdateStatus(IPowerSupplyStatus status)
		{
			var percentage = Math.Round(status.BatteryCharge * 100);
			var tooltip = string.Empty;

			RenderCharge(status.BatteryCharge, status.BatteryChargeStatus);

			if (status.IsOnline)
			{
				infoShown = false;
				warningShown = false;
				tooltip = text.Get(percentage == 100 ? TextKey.SystemControl_BatteryCharged : TextKey.SystemControl_BatteryCharging);
				tooltip = tooltip.Replace("%%CHARGE%%", percentage.ToString());
			}
			else
			{
				tooltip = text.Get(TextKey.SystemControl_BatteryRemainingCharge);
				tooltip = tooltip.Replace("%%CHARGE%%", percentage.ToString());
				tooltip = tooltip.Replace("%%HOURS%%", status.BatteryTimeRemaining.Hours.ToString());
				tooltip = tooltip.Replace("%%MINUTES%%", status.BatteryTimeRemaining.Minutes.ToString());

				HandleBatteryStatus(status.BatteryChargeStatus);
			}

			if (!infoShown && !warningShown)
			{
				Button.ToolTip = tooltip;
			}

			PowerPlug.Visibility = status.IsOnline ? Visibility.Visible : Visibility.Collapsed;
			Text.Text = tooltip;
			Warning.Visibility = status.BatteryChargeStatus == BatteryChargeStatus.Critical ? Visibility.Visible : Visibility.Collapsed;
			this.SetValue(System.Windows.Automation.AutomationProperties.NameProperty, tooltip);
		}

		private void RenderCharge(double charge, BatteryChargeStatus status)
		{
			var width = maxWidth * charge;

			BatteryCharge.Width = width > maxWidth ? maxWidth : (width < 0 ? 0 : width);

			switch (status)
			{
				case BatteryChargeStatus.Critical:
					BatteryCharge.Fill = Brushes.Red;
					break;
				case BatteryChargeStatus.Low:
					BatteryCharge.Fill = Brushes.Orange;
					break;
				default:
					BatteryCharge.Fill = initialBrush;
					break;
			}
		}

		private void HandleBatteryStatus(BatteryChargeStatus chargeStatus)
		{
			if (chargeStatus == BatteryChargeStatus.Low && !infoShown)
			{
				Button.ToolTip = text.Get(TextKey.SystemControl_BatteryChargeLowInfo);
				infoShown = true;
			}

			if (chargeStatus == BatteryChargeStatus.Critical && !warningShown)
			{
				Button.ToolTip = text.Get(TextKey.SystemControl_BatteryChargeCriticalWarning);
				warningShown = true;
			}
		}
	}
}
