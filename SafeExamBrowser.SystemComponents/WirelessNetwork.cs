/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.SystemComponents;
using SafeExamBrowser.Contracts.UserInterface.Shell;
using SimpleWifi;
using SimpleWifi.Win32;
using SimpleWifi.Win32.Interop;

namespace SafeExamBrowser.SystemComponents
{
	public class WirelessNetwork : ISystemComponent<ISystemWirelessNetworkControl>
	{
		private const int FIVE_SECONDS = 5000;
		private readonly object @lock = new object();

		private List<ISystemWirelessNetworkControl> controls;
		private List<WirelessNetworkDefinition> networks;
		private bool hasWifiAdapter;
		private ILogger logger;
		private IText text;
		private Timer timer;
		private Wifi wifi;

		public WirelessNetwork(ILogger logger, IText text)
		{
			this.controls = new List<ISystemWirelessNetworkControl>();
			this.logger = logger;
			this.networks = new List<WirelessNetworkDefinition>();
			this.text = text;
		}

		public void Initialize()
		{
			wifi = new Wifi();
			wifi.ConnectionStatusChanged += Wifi_ConnectionStatusChanged;
			hasWifiAdapter = !wifi.NoWifiAvailable && !IsTurnedOff();

			if (hasWifiAdapter)
			{
				UpdateAvailableNetworks();
				StartTimer();

				logger.Info("Started monitoring the wireless network adapter.");
			}
			else
			{
				logger.Info("Wireless networks cannot be monitored, as there is no hardware adapter available or it is turned off.");
			}
		}

		public void Register(ISystemWirelessNetworkControl control)
		{
			if (hasWifiAdapter)
			{
				control.HasWirelessNetworkAdapter = true;
				control.NetworkSelected += Control_NetworkSelected;
			}
			else
			{
				control.HasWirelessNetworkAdapter = false;
				control.SetInformation(text.Get(TextKey.SystemControl_WirelessNotAvailable));
			}

			controls.Add(control);

			if (hasWifiAdapter)
			{
				UpdateControls();
			}
		}

		public void Terminate()
		{
			if (timer != null)
			{
				timer.Stop();
				logger.Info("Stopped monitoring the wireless network adapter.");
			}

			foreach (var control in controls)
			{
				control.Close();
			}
		}

		private void Control_NetworkSelected(Guid id)
		{
			lock (@lock)
			{
				var network = networks.First(n => n.Id == id);

				try
				{
					var request = new AuthRequest(network.AccessPoint);

					logger.Info($"Attempting to connect to '{network.Name}'...");
					network.AccessPoint.ConnectAsync(request, false, (success) => AccessPoint_OnConnectComplete(network.Name, success));

					foreach (var control in controls)
					{
						control.IsConnecting = true;
					}
				}
				catch (Exception e)
				{
					logger.Error($"Failed to connect to wireless network '{network.Name}!'", e);
				}
			}
		}

		private void AccessPoint_OnConnectComplete(string name, bool success)
		{
			if (success)
			{
				logger.Info($"Successfully connected to wireless network '{name}'.");
			}
			else
			{
				logger.Error($"Failed to connect to wireless network '{name}!'");
			}

			foreach (var control in controls)
			{
				control.IsConnecting = false;
			}

			UpdateAvailableNetworks();
			UpdateControls();
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			UpdateAvailableNetworks();
			UpdateControls();
		}

		private void Wifi_ConnectionStatusChanged(object sender, WifiStatusEventArgs e)
		{
			UpdateAvailableNetworks();
			UpdateControls();
		}

		private bool IsTurnedOff()
		{
			try
			{
				var client = new WlanClient();

				foreach (var @interface in client.Interfaces)
				{
					foreach (var state in @interface.RadioState.PhyRadioState)
					{
						if (state.dot11SoftwareRadioState == Dot11RadioState.On && state.dot11HardwareRadioState == Dot11RadioState.On)
						{
							return false;
						}
					}
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to determine the radio state of the wireless adapter(s)! Assuming it is (all are) turned off...", e);
			}

			return true;
		}

		private void UpdateControls()
		{
			lock (@lock)
			{
				try
				{
					var currentNetwork = networks.FirstOrDefault(n => n.Status == WirelessNetworkStatus.Connected);

					foreach (var control in controls)
					{
						if (wifi.ConnectionStatus == WifiStatus.Disconnected)
						{
							control.SetInformation(text.Get(TextKey.SystemControl_WirelessDisconnected));
						}

						if (currentNetwork != null)
						{
							control.SetInformation(text.Get(TextKey.SystemControl_WirelessConnected).Replace("%%NAME%%", currentNetwork.Name));
						}

						control.NetworkStatus = ToStatus(wifi.ConnectionStatus);
						control.Update(networks.ToList());
					}
				}
				catch (Exception e)
				{
					logger.Error("Failed to update the wireless network adapter status!", e);
				}
			}
		}

		private void UpdateAvailableNetworks()
		{
			lock (@lock)
			{
				networks.Clear();

				foreach (var accessPoint in wifi.GetAccessPoints())
				{
					// The user may only connect to an already configured wireless network!
					if (accessPoint.HasProfile)
					{
						networks.Add(ToDefinition(accessPoint));
					}
				}
			}
		}

		private void StartTimer()
		{
			timer = new Timer(FIVE_SECONDS);
			timer.Elapsed += Timer_Elapsed;
			timer.AutoReset = true;
			timer.Start();
		}

		private WirelessNetworkDefinition ToDefinition(AccessPoint accessPoint)
		{
			return new WirelessNetworkDefinition
			{
				AccessPoint = accessPoint,
				Name = accessPoint.Name,
				SignalStrength = Convert.ToInt32(accessPoint.SignalStrength),
				Status = accessPoint.IsConnected ? WirelessNetworkStatus.Connected : WirelessNetworkStatus.Disconnected
			};
		}

		private WirelessNetworkStatus ToStatus(WifiStatus status)
		{
			return status == WifiStatus.Connected ? WirelessNetworkStatus.Connected : WirelessNetworkStatus.Disconnected;
		}
	}
}
