/*
 * Copyright (c) 2024 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.Network;
using SafeExamBrowser.SystemComponents.Contracts.Network.Events;
using SafeExamBrowser.WindowsApi.Contracts;
using SimpleWifi;
using SimpleWifi.Win32;
using SimpleWifi.Win32.Interop;
using Timer = System.Timers.Timer;

namespace SafeExamBrowser.SystemComponents.Network
{
	/// <summary>
	/// Switch to the following WiFi library:
	/// https://github.com/emoacht/ManagedNativeWifi
	/// https://www.nuget.org/packages/ManagedNativeWifi
	///
	/// Potentially useful: 
	/// https://learn.microsoft.com/en-us/dotnet/api/system.net.networkinformation.networkinterface?view=netframework-4.8
	/// </summary>
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
		public event CredentialsRequiredEventHandler CredentialsRequired;

		public NetworkAdapter(ILogger logger, INativeMethods nativeMethods)
		{
			this.logger = logger;
			this.nativeMethods = nativeMethods;
			this.wirelessNetworks = new List<WirelessNetwork>();
		}

		public void ConnectToWirelessNetwork(string name)
		{
			lock (@lock)
			{
				var network = wirelessNetworks.FirstOrDefault(n => n.Name == name);

				if (network != default)
				{
					try
					{
						var accessPoint = network.AccessPoint;
						var request = new AuthRequest(accessPoint);

						if (accessPoint.HasProfile || accessPoint.IsConnected || TryGetCredentials(request))
						{
							logger.Info($"Attempting to connect to wireless network '{network.Name}' with{(request.Password == default ? "out" : "")} credentials...");

							// TODO: Retry resp. alert of password error on failure and then ignore profile?!
							accessPoint.ConnectAsync(request, false, (success) => ConnectionAttemptCompleted(network.Name, success));
							Status = ConnectionStatus.Connecting;
						}
					}
					catch (Exception e)
					{
						logger.Error($"Failed to connect to wireless network '{network.Name}!'", e);
					}
				}
				else
				{
					logger.Warn($"Could not find wireless network '{name}'!");
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

		private void ConnectionAttemptCompleted(string name, bool success)
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

		private bool TryGetCredentials(AuthRequest request)
		{
			var args = new CredentialsRequiredEventArgs();

			CredentialsRequired?.Invoke(args);

			if (args.Success)
			{
				request.Password = args.Password;
				request.Username = args.Username;
			}

			return args.Success;
		}

		private void Update()
		{
			try
			{
				lock (@lock)
				{
					var current = default(WirelessNetwork);
					var hasInternet = nativeMethods.HasInternetConnection();
					var hasWireless = !wifi.NoWifiAvailable && !IsTurnedOff();
					var isConnecting = Status == ConnectionStatus.Connecting;
					var previousStatus = Status;

					wirelessNetworks.Clear();

					if (hasWireless)
					{
						foreach (var wirelessNetwork in wifi.GetAccessPoints().Select(a => ToWirelessNetwork(a)))
						{
							wirelessNetworks.Add(wirelessNetwork);

							if (wirelessNetwork.Status == ConnectionStatus.Connected)
							{
								current = wirelessNetwork;
							}
						}
					}

					Type = hasWireless ? ConnectionType.Wireless : (hasInternet ? ConnectionType.Wired : ConnectionType.Undefined);
					Status = hasInternet ? ConnectionStatus.Connected : (hasWireless && isConnecting ? ConnectionStatus.Connecting : ConnectionStatus.Disconnected);

					if (previousStatus != ConnectionStatus.Connected && Status == ConnectionStatus.Connected)
					{
						logger.Info($"Connection established ({Type}{(current != default ? $", {current.Name}" : "")}).");
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
