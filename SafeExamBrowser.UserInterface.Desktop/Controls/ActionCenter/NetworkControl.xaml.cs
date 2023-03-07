/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
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
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.Network;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Desktop.Controls.ActionCenter
{
	internal partial class NetworkControl : UserControl, ISystemControl
	{
		private readonly INetworkAdapter adapter;
		private readonly IText text;

		internal NetworkControl(INetworkAdapter adapter, IText text)
		{
			this.adapter = adapter;
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

			adapter.Changed += () => Dispatcher.InvokeAsync(Update);
			Button.Click += (o, args) => Popup.IsOpen = !Popup.IsOpen;
			var lastOpenedBySpacePress = false;
			Button.PreviewKeyDown += (o, args) =>
			{
				if (args.Key == System.Windows.Input.Key.Space)                 // for some reason, the popup immediately closes again if opened by a Space Bar key event - as a mitigation, we record the space bar event and leave the popup open for at least 3 seconds
				{
					lastOpenedBySpacePress = true;
				}
			};
			Button.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() =>
			{
				if (Popup.IsOpen && lastOpenedBySpacePress)
				{
					return;
				}
				Popup.IsOpen = Popup.IsMouseOver;
			}));
			Popup.MouseLeave += (o, args) => Task.Delay(250).ContinueWith(_ => Dispatcher.Invoke(() =>
			{
				if (Popup.IsOpen && lastOpenedBySpacePress)
				{
					return;
				}
				Popup.IsOpen = IsMouseOver;
			}));
			Popup.Opened += (o, args) =>
			{
				Grid.Background = Brushes.Gray;
				Task.Delay(100).ContinueWith((task) => Dispatcher.Invoke(() =>
				{
					if (WirelessNetworksStackPanel.Children.Count > 0)
					{
						var btn = WirelessNetworksStackPanel.Children[0] as NetworkButton;
						if (btn != null)
						{
							btn.SetFocus();
						}
					}
				}));
			};
			Popup.Closed += (o, args) =>
			{
				Grid.Background = originalBrush;
				lastOpenedBySpacePress = false;
			};
			WirelessIcon.Child = GetWirelessIcon(0);

			Update();
		}

		private void Update()
		{
			WirelessNetworksStackPanel.Children.Clear();

			foreach (var network in adapter.GetWirelessNetworks())
			{
				var button = new NetworkButton(network);

				button.NetworkSelected += (o, args) => adapter.ConnectToWirelessNetwork(network.Id);

				if (network.Status == ConnectionStatus.Connected)
				{
					WirelessIcon.Child = GetWirelessIcon(network.SignalStrength);
					UpdateText(text.Get(TextKey.SystemControl_NetworkWirelessConnected).Replace("%%NAME%%", network.Name));
				}

				WirelessNetworksStackPanel.Children.Add(button);
			}

			switch (adapter.Type)
			{
				case ConnectionType.Wired:
					Button.IsEnabled = false;
					UpdateText(text.Get(TextKey.SystemControl_NetworkWiredConnected));
					WiredIcon.Visibility = Visibility.Visible;
					WirelessIcon.Visibility = Visibility.Collapsed;
					break;
				case ConnectionType.Wireless:
					Button.IsEnabled = true;
					WiredIcon.Visibility = Visibility.Collapsed;
					WirelessIcon.Visibility = Visibility.Visible;
					break;
				default:
					Button.IsEnabled = false;
					UpdateText(text.Get(TextKey.SystemControl_NetworkNotAvailable));
					WiredIcon.Visibility = Visibility.Visible;
					WirelessIcon.Visibility = Visibility.Collapsed;
					break;
			}

			switch (adapter.Status)
			{
				case ConnectionStatus.Connected:
					NetworkStatusIcon.Rotation = 0;
					NetworkStatusIcon.Source = ImageAwesome.CreateImageSource(FontAwesomeIcon.Globe, Brushes.Green);
					NetworkStatusIcon.Spin = false;
					break;
				case ConnectionStatus.Connecting:
					UpdateText(text.Get(TextKey.SystemControl_NetworkWirelessConnecting));
					NetworkStatusIcon.Rotation = 0;
					NetworkStatusIcon.Source = ImageAwesome.CreateImageSource(FontAwesomeIcon.Cog, Brushes.DimGray);
					NetworkStatusIcon.Spin = true;
					NetworkStatusIcon.SpinDuration = 2;
					break;
				default:
					UpdateText(text.Get(TextKey.SystemControl_NetworkDisconnected));
					NetworkStatusIcon.Source = ImageAwesome.CreateImageSource(FontAwesomeIcon.Ban, Brushes.DarkOrange);
					NetworkStatusIcon.Spin = false;
					WirelessIcon.Child = GetWirelessIcon(0);
					break;
			}
		}

		private void UpdateText(string text)
		{
			Button.ToolTip = text;
			Text.Text = text;
			Button.SetValue(System.Windows.Automation.AutomationProperties.NameProperty, text);
		}

		private UIElement GetWirelessIcon(int signalStrength)
		{
			var icon = signalStrength > 66 ? "100" : (signalStrength > 33 ? "66" : (signalStrength > 0 ? "33" : "0"));
			var uri = new Uri($"pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/WiFi_Light_{icon}.xaml");
			var resource = new XamlIconResource { Uri = uri };

			return IconResourceLoader.Load(resource);
		}
	}
}
