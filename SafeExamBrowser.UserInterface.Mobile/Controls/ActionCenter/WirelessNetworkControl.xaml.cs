/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FontAwesome.WPF;
using SafeExamBrowser.Applications.Contracts.Resources.Icons;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.WirelessNetwork;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Mobile.Controls.ActionCenter
{
	internal partial class WirelessNetworkControl : UserControl, ISystemControl
	{
		private IWirelessAdapter wirelessAdapter;
		private IText text;

		internal WirelessNetworkControl(IWirelessAdapter wirelessAdapter, IText text)
		{
			this.wirelessAdapter = wirelessAdapter;
			this.text = text;

			InitializeComponent();
			InitializeWirelessNetworkControl();
		}

		public void Close()
		{
			Dispatcher.InvokeAsync(() => Popup.IsOpen = false);
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

			if (wirelessAdapter.IsAvailable)
			{
				wirelessAdapter.NetworksChanged += WirelessAdapter_NetworksChanged;
				wirelessAdapter.StatusChanged += WirelessAdapter_StatusChanged;
				UpdateNetworks();
			}
			else
			{
				Button.IsEnabled = false;
				NoAdapterIcon.Visibility = Visibility.Visible;
				UpdateText(text.Get(TextKey.SystemControl_WirelessNotAvailable));
			}
		}

		private void WirelessAdapter_NetworksChanged()
		{
			Dispatcher.InvokeAsync(UpdateNetworks);
		}

		private void WirelessAdapter_StatusChanged(WirelessNetworkStatus status)
		{
			Dispatcher.InvokeAsync(() => UpdateStatus(status));
		}

		private void UpdateNetworks()
		{
			var status = WirelessNetworkStatus.Disconnected;

			NetworksStackPanel.Children.Clear();

			foreach (var network in wirelessAdapter.GetNetworks())
			{
				var button = new WirelessNetworkButton(network);

				button.NetworkSelected += (o, args) => wirelessAdapter.Connect(network.Id);

				if (network.Status == WirelessNetworkStatus.Connected)
				{
					status = WirelessNetworkStatus.Connected;
					SignalStrengthIcon.Child = GetIcon(network.SignalStrength);
					UpdateText(text.Get(TextKey.SystemControl_WirelessConnected).Replace("%%NAME%%", network.Name));
				}

				NetworksStackPanel.Children.Add(button);
			}

			UpdateStatus(status);
		}

		private void UpdateStatus(WirelessNetworkStatus status)
		{
			LoadingIcon.Visibility = Visibility.Collapsed;
			SignalStrengthIcon.Visibility = Visibility.Visible;
			NetworkStatusIcon.Visibility = Visibility.Visible;

			switch (status)
			{
				case WirelessNetworkStatus.Connected:
					NetworkStatusIcon.Source = ImageAwesome.CreateImageSource(FontAwesomeIcon.Check, Brushes.Green);
					break;
				case WirelessNetworkStatus.Connecting:
					LoadingIcon.Visibility = Visibility.Visible;
					SignalStrengthIcon.Visibility = Visibility.Collapsed;
					NetworkStatusIcon.Visibility = Visibility.Collapsed;
					UpdateText(text.Get(TextKey.SystemControl_WirelessConnecting));
					break;
				default:
					NetworkStatusIcon.Source = ImageAwesome.CreateImageSource(FontAwesomeIcon.Close, Brushes.Orange);
					SignalStrengthIcon.Child = GetIcon(0);
					UpdateText(text.Get(TextKey.SystemControl_WirelessDisconnected));
					break;
			}
		}

		private void UpdateText(string text)
		{
			Button.ToolTip = text;
			Text.Text = text;
		}

		private UIElement GetIcon(int signalStrength)
		{
			var icon = signalStrength > 66 ? "100" : (signalStrength > 33 ? "66" : (signalStrength > 0 ? "33" : "0"));
			var uri = new Uri($"pack://application:,,,/SafeExamBrowser.UserInterface.Mobile;component/Images/WiFi_Light_{icon}.xaml");
			var resource = new XamlIconResource { Uri = uri };

			return IconResourceLoader.Load(resource);
		}
	}
}
