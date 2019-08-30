/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;

namespace SafeExamBrowser.UserInterface.Mobile.Controls
{
	public partial class TaskbarPowerSupplyControl : UserControl, ISystemPowerSupplyControl
	{
		private double BATTERY_CHARGE_MAX_WIDTH;

		public TaskbarPowerSupplyControl()
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
			Dispatcher.InvokeAsync(() =>
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
			Dispatcher.InvokeAsync(() => PowerPlug.Visibility = connected ? Visibility.Visible : Visibility.Collapsed);
		}

		public void SetInformation(string text)
		{
			Dispatcher.InvokeAsync(() => Button.ToolTip = text);
		}

		public void ShowCriticalBatteryWarning(string warning)
		{
			Dispatcher.InvokeAsync(() => ShowPopup(warning));
		}

		public void ShowLowBatteryInfo(string info)
		{
			Dispatcher.InvokeAsync(() => ShowPopup(info));
		}

		private void ShowPopup(string text)
		{
			Popup.IsOpen = true;
			PopupText.Text = text;
			Background = Brushes.LightGray;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Popup.IsOpen = false;
			Background = Brushes.Transparent;
		}
	}
}
