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
using SafeExamBrowser.Contracts.SystemComponents;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;
using SafeExamBrowser.Contracts.UserInterface.Taskbar.Events;
using SafeExamBrowser.UserInterface.Desktop.Utilities;

namespace SafeExamBrowser.UserInterface.Desktop.Controls
{
	public partial class WirelessNetworkControl : UserControl, ISystemWirelessNetworkControl
	{
		public bool HasWirelessNetworkAdapter
		{
			set
			{
				Dispatcher.InvokeAsync(() =>
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
				Dispatcher.InvokeAsync(() =>
				{
					var icon = value == WirelessNetworkStatus.Connected ? FontAwesomeIcon.Check : FontAwesomeIcon.Close;
					var brush = value == WirelessNetworkStatus.Connected ? Brushes.Green : Brushes.Orange;

					NetworkStatusIcon.Source = ImageAwesome.CreateImageSource(icon, brush);
				});
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
			Dispatcher.InvokeAsync(() => Button.ToolTip = text);
		}

		public void Update(IEnumerable<IWirelessNetwork> networks)
		{
			Dispatcher.InvokeAsync(() =>
			{
				NetworksStackPanel.Children.Clear();

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
						SignalStrengthIcon.Child = GetIcon(network.SignalStrength);
					}

					NetworksStackPanel.Children.Add(button);
				}
			});
		}

		private void InitializeWirelessNetworkControl()
		{
			var originalBrush = Button.Background;

			SignalStrengthIcon.Child = GetIcon(0);
			Button.Click += (o, args) => Popup.IsOpen = !Popup.IsOpen;
			Button.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() => Popup.IsOpen = Popup.IsMouseOver));
			Popup.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() => Popup.IsOpen = IsMouseOver));

			Popup.Opened += (o, args) =>
			{
				Background = Brushes.LightGray;
				Button.Background = Brushes.LightGray;
			};

			Popup.Closed += (o, args) =>
			{
				Background = originalBrush;
				Button.Background = originalBrush;
			};
		}

		private UIElement GetIcon(int signalStrength)
		{
			var icon = signalStrength > 66 ? "100" : (signalStrength > 33 ? "66" : (signalStrength > 0 ? "33" : "0"));
			var uri = new Uri($"pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/WiFi_{icon}.xaml");
			var resource = new XamlIconResource(uri);

			return IconResourceLoader.Load(resource);
		}
	}
}
