/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SafeExamBrowser.Contracts.SystemComponents;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;

namespace SafeExamBrowser.UserInterface.Controls
{
	public partial class PowerSupplyControl : UserControl, ISystemPowerSupplyControl
	{
		private double BATTERY_CHARGE_MAX_WIDTH;

		public PowerSupplyControl()
		{
			InitializeComponent();
			BATTERY_CHARGE_MAX_WIDTH = BatteryCharge.Width;
		}

		public void Close()
		{
			Popup.IsOpen = false;
		}

		public void SetBatteryCharge(double charge, BatteryChargeStatus status)
		{
			Dispatcher.Invoke(() =>
			{
				var width = BATTERY_CHARGE_MAX_WIDTH * charge;

				width = width > BATTERY_CHARGE_MAX_WIDTH ? BATTERY_CHARGE_MAX_WIDTH : width;
				width = width < 0 ? 0 : width;

				BatteryCharge.Width = width;
				BatteryCharge.Fill = status == BatteryChargeStatus.Low ? Brushes.Orange : BatteryCharge.Fill;
				BatteryCharge.Fill = status == BatteryChargeStatus.Critical ? Brushes.Red : BatteryCharge.Fill;
				Warning.Visibility = status == BatteryChargeStatus.Critical ? Visibility.Visible : Visibility.Collapsed;
			});
		}

		public void SetPowerGridConnection(bool connected)
		{
			Dispatcher.Invoke(() => PowerPlug.Visibility = connected ? Visibility.Visible : Visibility.Collapsed);
		}

		public void SetTooltip(string text)
		{
			Dispatcher.Invoke(() => Button.ToolTip = text);
		}

		public void ShowCriticalBatteryWarning(string warning)
		{
			Dispatcher.Invoke(() => ShowPopup(warning));
		}

		public void ShowLowBatteryInfo(string info)
		{
			Dispatcher.Invoke(() => ShowPopup(info));
		}

		private void ShowPopup(string text)
		{
			Popup.IsOpen = true;
			PopupText.Text = text;
			Background = (Brush) new BrushConverter().ConvertFrom("#2AFFFFFF");
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Popup.IsOpen = false;
			Background = (Brush) new BrushConverter().ConvertFrom("#00000000");
		}
	}
}
