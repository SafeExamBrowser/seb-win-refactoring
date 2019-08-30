/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FontAwesome.WPF;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Contracts.Shell.Events;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Mobile.Controls
{
	public partial class ActionCenterWirelessNetworkControl : UserControl, ISystemWirelessNetworkControl
	{
		public bool HasWirelessNetworkAdapter
		{
			set
			{
				Dispatcher.Invoke(() =>
				{
					Button.IsEnabled = value;
					NoAdapterIcon.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
				});
			}
		}

		public bool IsConnecting
		{
			set
			{
				Dispatcher.Invoke(() =>
				{
					LoadingIcon.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
					SignalStrengthIcon.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
					NetworkStatusIcon.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
				});
			}
		}

		public WirelessNetworkStatus NetworkStatus
		{
			set
			{
				Dispatcher.Invoke(() =>
				{
					var icon = value == WirelessNetworkStatus.Connected ? FontAwesomeIcon.Check : FontAwesomeIcon.Close;
					var brush = value == WirelessNetworkStatus.Connected ? Brushes.Green : Brushes.Orange;

					if (value == WirelessNetworkStatus.Disconnected)
					{
						SignalStrengthIcon.Child = GetIcon(0);
					}

					NetworkStatusIcon.Source = ImageAwesome.CreateImageSource(icon, brush);
				});
			}
		}

		public event WirelessNetworkSelectedEventHandler NetworkSelected;

		public ActionCenterWirelessNetworkControl()
		{
			InitializeComponent();
			InitializeWirelessNetworkControl();
		}

		public void Close()
		{
			Dispatcher.Invoke(() => Popup.IsOpen = false);
		}

		public void SetInformation(string text)
		{
			Dispatcher.Invoke(() =>
			{
				Button.ToolTip = text;
				Text.Text = text;
			});
		}

		public void Update(IEnumerable<IWirelessNetwork> networks)
		{
			Dispatcher.Invoke(() =>
			{
				NetworksStackPanel.Children.Clear();

				foreach (var network in networks)
				{
					var button = new ActionCenterWirelessNetworkButton(network);
					var isCurrent = network.Status == WirelessNetworkStatus.Connected;

					button.IsCurrent = isCurrent;
					button.NetworkName = network.Name;
					button.SignalStrength = network.SignalStrength;
					button.NetworkSelected += (id) => NetworkSelected?.Invoke(id);

					if (isCurrent)
					{
						NetworkStatus = network.Status;
						SignalStrengthIcon.Child = GetIcon(network.SignalStrength);
					}

					NetworksStackPanel.Children.Add(button);
				}
			});
		}

		private void InitializeWirelessNetworkControl()
		{
			var originalBrush = Grid.Background;

			SignalStrengthIcon.Child = GetIcon(0);
			Button.Click += (o, args) => Popup.IsOpen = !Popup.IsOpen;
			Button.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() => Popup.IsOpen = Popup.IsMouseOver));
			Popup.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() => Popup.IsOpen = IsMouseOver));
			Popup.Opened += (o, args) => Grid.Background = Brushes.Gray;
			Popup.Closed += (o, args) => Grid.Background = originalBrush;
		}

		private UIElement GetIcon(int signalStrength)
		{
			var icon = signalStrength > 66 ? "100" : (signalStrength > 33 ? "66" : (signalStrength > 0 ? "33" : "0"));
			var uri = new Uri($"pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/WiFi_Light_{icon}.xaml");
			var resource = new XamlIconResource(uri);

			return IconResourceLoader.Load(resource);
		}
	}
}
