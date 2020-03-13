/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.WirelessNetwork;
using SafeExamBrowser.SystemComponents.Contracts.WirelessNetwork.Events;
using SimpleWifi;
using SimpleWifi.Win32;
using SimpleWifi.Win32.Interop;

namespace SafeExamBrowser.SystemComponents.WirelessNetwork
{
	public class WirelessAdapter : IWirelessAdapter
	{
		private readonly object @lock = new object();

		private List<WirelessNetwork> networks;
		private ILogger logger;
		private Timer timer;
		private Wifi wifi;

		public bool IsAvailable { get; private set; }

		public event NetworksChangedEventHandler NetworksChanged;
		public event StatusChangedEventHandler StatusChanged;

		public WirelessAdapter(ILogger logger)
		{
			this.logger = logger;
			this.networks = new List<WirelessNetwork>();
		}

		public void Connect(Guid id)
		{
			lock (@lock)
			{
				var network = networks.FirstOrDefault(n => n.Id == id);

				if (network != default(WirelessNetwork))
				{
					try
					{
						var request = new AuthRequest(network.AccessPoint);

						logger.Info($"Attempting to connect to '{network.Name}'...");
						network.AccessPoint.ConnectAsync(request, false, (success) => AccessPoint_OnConnectCompleted(network.Name, success));
						StatusChanged?.Invoke(WirelessNetworkStatus.Connecting);
					}
					catch (Exception e)
					{
						logger.Error($"Failed to connect to wireless network '{network.Name}!'", e);
					}
				}
				else
				{
					logger.Warn($"Could not find network with id '{id}'!");
				}
			}
		}

		public IEnumerable<IWirelessNetwork> GetNetworks()
		{
			lock (@lock)
			{
				return new List<WirelessNetwork>(networks);
			}
		}

		public void Initialize()
		{
			const int FIVE_SECONDS = 5000;

			wifi = new Wifi();
			wifi.ConnectionStatusChanged += Wifi_ConnectionStatusChanged;
			IsAvailable = !wifi.NoWifiAvailable && !IsTurnedOff();

			if (IsAvailable)
			{
				UpdateAvailableNetworks();

				timer = new Timer(FIVE_SECONDS);
				timer.Elapsed += Timer_Elapsed;
				timer.AutoReset = true;
				timer.Start();

				logger.Info("Started monitoring the wireless network adapter.");
			}
			else
			{
				logger.Info("Wireless networks cannot be monitored, as there is no hardware adapter available or it is turned off.");
			}
		}

		public void Terminate()
		{
			if (timer != null)
			{
				timer.Stop();
				logger.Info("Stopped monitoring the wireless network adapter.");
			}
		}

		private void AccessPoint_OnConnectCompleted(string name, bool success)
		{
			if (success)
			{
				logger.Info($"Successfully connected to wireless network '{name}'.");
			}
			else
			{
				logger.Error($"Failed to connect to wireless network '{name}!'");
			}

			UpdateAvailableNetworks();
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			UpdateAvailableNetworks();
		}

		private void Wifi_ConnectionStatusChanged(object sender, WifiStatusEventArgs e)
		{
			UpdateAvailableNetworks();
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

		private void UpdateAvailableNetworks()
		{
			lock (@lock)
			{
				try
				{
					networks.Clear();

					foreach (var accessPoint in wifi.GetAccessPoints())
					{
						// The user may only connect to an already configured or connected wireless network!
						if (accessPoint.HasProfile || accessPoint.IsConnected)
						{
							networks.Add(ToNetwork(accessPoint));
						}
					}

					NetworksChanged?.Invoke();
				}
				catch (Exception e)
				{
					logger.Error("Failed to update available networks!", e);
				}
			}
		}

		private WirelessNetwork ToNetwork(AccessPoint accessPoint)
		{
			return new WirelessNetwork
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
