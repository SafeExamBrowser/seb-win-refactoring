/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using SafeExamBrowser.Contracts.SystemComponents;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;

namespace SafeExamBrowser.UserInterface.Classic.Controls
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
			Dispatcher.BeginInvoke(new Action(() =>
			{
				var width = BATTERY_CHARGE_MAX_WIDTH * charge;

				width = width > BATTERY_CHARGE_MAX_WIDTH ? BATTERY_CHARGE_MAX_WIDTH : width;
				width = width < 0 ? 0 : width;

				BatteryCharge.Width = width;
				BatteryCharge.Fill = status == BatteryChargeStatus.Low ? Brushes.Orange : BatteryCharge.Fill;
				BatteryCharge.Fill = status == BatteryChargeStatus.Critical ? Brushes.Red : BatteryCharge.Fill;
				Warning.Visibility = status == BatteryChargeStatus.Critical ? Visibility.Visible : Visibility.Collapsed;
			}));
		}

		public void SetPowerGridConnection(bool connected)
		{
			Dispatcher.BeginInvoke(new Action(() => PowerPlug.Visibility = connected ? Visibility.Visible : Visibility.Collapsed));
		}

		public void SetTooltip(string text)
		{
			Dispatcher.BeginInvoke(new Action(() => Button.ToolTip = text));
		}

		public void ShowCriticalBatteryWarning(string warning)
		{
			Dispatcher.BeginInvoke(new Action(() => ShowPopup(warning)));
		}

		public void ShowLowBatteryInfo(string info)
		{
			Dispatcher.BeginInvoke(new Action(() => ShowPopup(info)));
		}

		private void ShowPopup(string text)
		{
			Popup.IsOpen = true;
			PopupText.Text = text;
			Background = Brushes.LightGray;
			Button.IsEnabled = true;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Popup.IsOpen = false;
			Background = Brushes.Transparent;
			Button.IsEnabled = false;
		}
	}
}
