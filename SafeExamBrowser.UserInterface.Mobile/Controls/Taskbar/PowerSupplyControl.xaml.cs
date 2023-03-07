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
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.PowerSupply;
using SafeExamBrowser.UserInterface.Contracts.Shell;

namespace SafeExamBrowser.UserInterface.Mobile.Controls.Taskbar
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
			Dispatcher.InvokeAsync(ClosePopup);
		}

		public void SetInformation(string text)
		{
			Dispatcher.InvokeAsync(() => PopupText.Text = text);
		}

		private void InitializePowerSupplyControl()
		{
			initialBrush = BatteryCharge.Fill;
			maxWidth = BatteryCharge.Width;
			powerSupply.StatusChanged += PowerSupply_StatusChanged;
			Popup.CustomPopupPlacementCallback = new CustomPopupPlacementCallback(Popup_PlacementCallback);
			UpdateStatus(powerSupply.GetStatus());
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			ClosePopup();
		}

		private CustomPopupPlacement[] Popup_PlacementCallback(Size popupSize, Size targetSize, Point offset)
		{
			return new[]
			{
				new CustomPopupPlacement(new Point(targetSize.Width / 2 - popupSize.Width / 2, -popupSize.Height), PopupPrimaryAxis.None)
			};
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

				ClosePopup();
			}
			else
			{
				tooltip = text.Get(TextKey.SystemControl_BatteryRemainingCharge);
				tooltip = tooltip.Replace("%%CHARGE%%", percentage.ToString());
				tooltip = tooltip.Replace("%%HOURS%%", status.BatteryTimeRemaining.Hours.ToString());
				tooltip = tooltip.Replace("%%MINUTES%%", status.BatteryTimeRemaining.Minutes.ToString());

				HandleBatteryStatus(status.BatteryChargeStatus);
			}

			Button.ToolTip = tooltip;
			PowerPlug.Visibility = status.IsOnline ? Visibility.Visible : Visibility.Collapsed;
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
				ShowPopup(text.Get(TextKey.SystemControl_BatteryChargeLowInfo));
				infoShown = true;
			}

			if (chargeStatus == BatteryChargeStatus.Critical && !warningShown)
			{
				ShowPopup(text.Get(TextKey.SystemControl_BatteryChargeCriticalWarning));
				warningShown = true;
			}
		}

		private void ShowPopup(string text)
		{
			Popup.IsOpen = true;
			PopupText.Text = text;
			Background = Brushes.LightGray;
		}

		private void ClosePopup()
		{
			Popup.IsOpen = false;
			Background = Brushes.Transparent;
		}
	}
}
