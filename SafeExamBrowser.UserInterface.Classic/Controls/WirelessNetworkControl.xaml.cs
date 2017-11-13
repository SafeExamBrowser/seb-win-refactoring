/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using SafeExamBrowser.Contracts.SystemComponents;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;

namespace SafeExamBrowser.UserInterface.Classic.Controls
{
	public partial class WirelessNetworkControl : UserControl, ISystemWirelessNetworkControl
	{
		public bool HasWirelessNetworkAdapter
		{
			set
			{
				Dispatcher.BeginInvoke(new Action(() =>
				{
					Button.IsEnabled = value;
				}));
			}
		}

		public WirelessNetworkStatus NetworkStatus
		{
			set
			{
				Dispatcher.BeginInvoke(new Action(() =>
				{
					NetworkStatusIcon.Text = value == WirelessNetworkStatus.Connected ? "&#10004;" : "&#10060;";
					NetworkStatusIcon.Foreground = value == WirelessNetworkStatus.Connected ? Brushes.Green : Brushes.Red;
				}));
			}
		}

		public event WirelessNetworkSelectedEventHandler NetworkSelected;

		public WirelessNetworkControl()
		{
			InitializeComponent();
			InitializeWirelessNetworkControl();
		}

		public void Close()
		{
			Popup.IsOpen = false;
		}

		public void SetTooltip(string text)
		{
			Dispatcher.BeginInvoke(new Action(() => Button.ToolTip = text));
		}

		public void Update(IEnumerable<IWirelessNetwork> networks)
		{
			Dispatcher.BeginInvoke(new Action(() =>
			{
				foreach (var network in networks)
				{
					var button = new WirelessNetworkButton();
					var isCurrent = network.Status == WirelessNetworkStatus.Connected;

					button.Click += (o, args) =>
					{
						NetworkSelected?.Invoke(network);
					};
					button.IsCurrent = isCurrent;
					button.NetworkName = network.Name;
					button.SignalStrength = network.SignalStrength;

					if (isCurrent)
					{
						NetworkStatus = network.Status;
					}

					NetworksStackPanel.Children.Add(button);
				}
			}));
		}

		private void InitializeWirelessNetworkControl()
		{
			var originalBrush = Button.Background;

			Button.Click += (o, args) => Popup.IsOpen = !Popup.IsOpen;
			Button.MouseLeave += (o, args) => Popup.IsOpen = Popup.IsMouseOver;
			Popup.MouseLeave += (o, args) => Popup.IsOpen = IsMouseOver;

			Popup.Opened += (o, args) =>
			{
				Background = Brushes.LightBlue;
				Button.Background = Brushes.LightBlue;
			};

			Popup.Closed += (o, args) =>
			{
				Background = originalBrush;
				Button.Background = originalBrush;
			};
		}
	}
}
