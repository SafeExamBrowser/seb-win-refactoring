/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Timers;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.Network;
using SafeExamBrowser.SystemComponents.Contracts.Network.Events;
using SafeExamBrowser.WindowsApi.Contracts;
using SimpleWifi;
using SimpleWifi.Win32;
using SimpleWifi.Win32.Interop;

namespace SafeExamBrowser.SystemComponents.Network
{
	public class NetworkAdapter : INetworkAdapter
	{
		private readonly object @lock = new object();
		private readonly ILogger logger;
		private readonly INativeMethods nativeMethods;
		private readonly List<WirelessNetwork> wirelessNetworks;

		private Timer timer;
		private Wifi wifi;

		public ConnectionStatus Status { get; private set; }
		public ConnectionType Type { get; private set; }

		public event ChangedEventHandler Changed;

		public NetworkAdapter(ILogger logger, INativeMethods nativeMethods)
		{
			this.logger = logger;
			this.nativeMethods = nativeMethods;
			this.wirelessNetworks = new List<WirelessNetwork>();
		}

		public void ConnectToWirelessNetwork(Guid id)
		{
			lock (@lock)
			{
				var network = wirelessNetworks.FirstOrDefault(n => n.Id == id);

				if (network != default)
				{
					try
					{
						var request = new AuthRequest(network.AccessPoint);

						logger.Info($"Attempting to connect to '{network.Name}'...");

						network.AccessPoint.ConnectAsync(request, false, (success) => AccessPoint_OnConnectCompleted(network.Name, success));
						Status = ConnectionStatus.Connecting;
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

			Changed?.Invoke();
		}

		public IEnumerable<IWirelessNetwork> GetWirelessNetworks()
		{
			lock (@lock)
			{
				return new List<WirelessNetwork>(wirelessNetworks);
			}
		}

		public void Initialize()
		{
			const int FIVE_SECONDS = 5000;

			NetworkChange.NetworkAddressChanged += (o, args) => Update();
			NetworkChange.NetworkAvailabilityChanged += (o, args) => Update();

			wifi = new Wifi();
			wifi.ConnectionStatusChanged += (o, args) => Update();

			timer = new Timer(FIVE_SECONDS);
			timer.Elapsed += (o, args) => Update();
			timer.AutoReset = true;
			timer.Start();

			Update();

			logger.Info("Started monitoring the network adapter.");
		}

		public void Terminate()
		{
			if (timer != null)
			{
				timer.Stop();
				logger.Info("Stopped monitoring the network adapter.");
			}
		}

		private void AccessPoint_OnConnectCompleted(string name, bool success)
		{
			lock (@lock)
			{
				// This handler seems to be called before the connection has been fully established, thus we don't yet set the status to connected...
				Status = success ? ConnectionStatus.Connecting : ConnectionStatus.Disconnected;
			}

			if (success)
			{
				logger.Info($"Successfully connected to wireless network '{name}'.");
			}
			else
			{
				logger.Error($"Failed to connect to wireless network '{name}!'");
			}
		}

		private void Update()
		{
			try
			{
				lock (@lock)
				{
					var hasInternet = nativeMethods.HasInternetConnection();
					var hasWireless = !wifi.NoWifiAvailable && !IsTurnedOff();
					var isConnecting = Status == ConnectionStatus.Connecting;
					var previousStatus = Status;

					wirelessNetworks.Clear();

					if (hasWireless)
					{
						foreach (var accessPoint in wifi.GetAccessPoints())
						{
							// The user may only connect to an already configured or connected wireless network!
							if (accessPoint.HasProfile || accessPoint.IsConnected)
							{
								wirelessNetworks.Add(ToWirelessNetwork(accessPoint));
							}
						}
					}

					Type = hasWireless ? ConnectionType.Wireless : (hasInternet ? ConnectionType.Wired : ConnectionType.Undefined);
					Status = hasInternet ? ConnectionStatus.Connected : (hasWireless && isConnecting ? ConnectionStatus.Connecting : ConnectionStatus.Disconnected);

					if (previousStatus != ConnectionStatus.Connected && Status == ConnectionStatus.Connected)
					{
						logger.Info("Connection established.");
					}

					if (previousStatus != ConnectionStatus.Disconnected && Status == ConnectionStatus.Disconnected)
					{
						logger.Info("Connection lost.");
					}
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to update network adapter!", e);
			}

			Changed?.Invoke();
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

		private WirelessNetwork ToWirelessNetwork(AccessPoint accessPoint)
		{
			return new WirelessNetwork
			{
				AccessPoint = accessPoint,
				Name = accessPoint.Name,
				SignalStrength = Convert.ToInt32(accessPoint.SignalStrength),
				Status = accessPoint.IsConnected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected
			};
		}
	}
}
