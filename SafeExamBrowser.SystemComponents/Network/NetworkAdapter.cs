/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts.Network;
using SafeExamBrowser.SystemComponents.Contracts.Network.Events;
using SafeExamBrowser.WindowsApi.Contracts;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFi;
using Windows.Foundation;
using Windows.Networking.Connectivity;
using Windows.Security.Credentials;
using Timer = System.Timers.Timer;

namespace SafeExamBrowser.SystemComponents.Network
{
	public class NetworkAdapter : INetworkAdapter
	{
		private readonly object @lock = new object();

		private readonly ConcurrentDictionary<string, object> attempts;
		private readonly ILogger logger;
		private readonly INativeMethods nativeMethods;
		private readonly List<WirelessNetwork> wirelessNetworks;

		private WiFiAdapter adapter;
		private Timer timer;

		private bool HasWirelessAdapter => adapter != default;

		public ConnectionStatus Status { get; private set; }
		public ConnectionType Type { get; private set; }

		public event ChangedEventHandler Changed;
		public event CredentialsRequiredEventHandler CredentialsRequired;

		public NetworkAdapter(ILogger logger, INativeMethods nativeMethods)
		{
			this.attempts = new ConcurrentDictionary<string, object>();
			this.logger = logger;
			this.nativeMethods = nativeMethods;
			this.wirelessNetworks = new List<WirelessNetwork>();
		}

		public void ConnectToWirelessNetwork(string name)
		{
			var isFirstAttempt = !attempts.TryGetValue(name, out _);
			var network = default(WiFiAvailableNetwork);

			lock (@lock)
			{
				network = wirelessNetworks.FirstOrDefault(n => n.Name == name)?.Network;
			}

			if (network != default)
			{
				if (isFirstAttempt || network.IsOpen())
				{
					ConnectAutomatically(network);
				}
				else
				{
					ConnectWithAuthentication(network);
				}

				Changed?.Invoke();
			}
			else
			{
				logger.Warn($"Could not find wireless network '{name}'!");
			}
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

			timer = new Timer(FIVE_SECONDS);
			timer.Elapsed += (o, args) => Update();
			timer.AutoReset = true;

			InitializeAdapter();

			NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
			NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
			NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;

			Update();

			logger.Info("Started monitoring the network adapter.");
		}

		public void StartWirelessNetworkScanning()
		{
			timer?.Start();

			if (HasWirelessAdapter)
			{
				_ = adapter.ScanAsync();
			}
		}

		public void StopWirelessNetworkScanning()
		{
			timer?.Stop();
		}

		public void Terminate()
		{
			NetworkChange.NetworkAddressChanged -= NetworkChange_NetworkAddressChanged;
			NetworkChange.NetworkAvailabilityChanged -= NetworkChange_NetworkAvailabilityChanged;
			NetworkInformation.NetworkStatusChanged -= NetworkInformation_NetworkStatusChanged;

			if (HasWirelessAdapter)
			{
				adapter.AvailableNetworksChanged -= Adapter_AvailableNetworksChanged;
			}

			if (timer != default)
			{
				timer.Stop();
			}

			logger.Info("Stopped monitoring the network adapter.");
		}

		private void Adapter_AvailableNetworksChanged(WiFiAdapter sender, object args)
		{
			Update(false);
		}

		private void Adapter_ConnectCompleted(WiFiAvailableNetwork network, IAsyncOperation<WiFiConnectionResult> operation, AsyncStatus status)
		{
			var connectionStatus = default(WiFiConnectionStatus?);

			if (status == AsyncStatus.Completed)
			{
				connectionStatus = operation.GetResults()?.ConnectionStatus;
			}
			else
			{
				logger.Error($"Failed to complete connection operation! Status: {status}.");
			}

			if (connectionStatus == WiFiConnectionStatus.Success)
			{
				attempts.TryRemove(network.Ssid, out _);
				logger.Info($"Successfully connected to wireless network {network.ToLogString()}.");
			}
			else if (connectionStatus == WiFiConnectionStatus.InvalidCredential)
			{
				attempts.TryAdd(network.Ssid, default);
				logger.Info($"Failed to connect to wireless network {network.ToLogString()} due to invalid credentials. Retrying...");
				Task.Run(() => ConnectToWirelessNetwork(network.Ssid));
			}
			else
			{
				Status = ConnectionStatus.Disconnected;
				logger.Error($"Failed to connect to wireless network {network.ToLogString()}! Reason: {connectionStatus}.");
			}

			Update();
		}

		private void ConnectAutomatically(WiFiAvailableNetwork network)
		{
			logger.Info($"Attempting to automatically connect to {(network.IsOpen() ? "open" : "protected")} wireless network {network.ToLogString()}...");

			adapter.ConnectAsync(network, WiFiReconnectionKind.Automatic).Completed = (o, s) => Adapter_ConnectCompleted(network, o, s);
			Status = ConnectionStatus.Connecting;
		}

		private void ConnectWithAuthentication(WiFiAvailableNetwork network)
		{
			if (TryGetCredentials(network.Ssid, out var credentials))
			{
				logger.Info($"Attempting to connect to protected wirless network {network.ToLogString()}...");

				adapter.ConnectAsync(network, WiFiReconnectionKind.Automatic, credentials).Completed = (o, s) => Adapter_ConnectCompleted(network, o, s);
				Status = ConnectionStatus.Connecting;
			}
			else
			{
				Status = ConnectionStatus.Disconnected;
				Update();
			}
		}

		private void InitializeAdapter()
		{
			try
			{
				// Requesting access is required as of fall 2024 and must be granted manually by the user, otherwise all wireless functionality will
				// be denied by the system (see also https://learn.microsoft.com/en-us/windows/win32/nativewifi/wi-fi-access-location-changes).
				var task = WiFiAdapter.RequestAccessAsync().AsTask();
				var status = task.GetAwaiter().GetResult();

				if (status == WiFiAccessStatus.Allowed)
				{
					var findAll = DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector()).AsTask();
					var devices = findAll.GetAwaiter().GetResult();

					if (devices.Any())
					{
						var id = devices.First().Id;
						var getById = WiFiAdapter.FromIdAsync(id).AsTask();

						logger.Debug($"Found {devices.Count()} wireless network adapter(s).");

						adapter = getById.GetAwaiter().GetResult();
						adapter.AvailableNetworksChanged += Adapter_AvailableNetworksChanged;

						logger.Debug($"Successfully initialized wireless network adapter '{id}'.");
					}
					else
					{
						logger.Info("Could not find a wireless network adapter.");
					}
				}
				else
				{
					logger.Error($"Access to the wireless network adapter has been denied ({status})!");
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to initialize wireless network adapter!", e);
			}
		}

		private void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
		{
			logger.Debug("Network address changed.");
			Update();
		}

		private void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
		{
			logger.Debug($"Network availability changed ({(e.IsAvailable ? "available" : "unavailable")}).");
			Update();
		}

		private void NetworkInformation_NetworkStatusChanged(object sender)
		{
			logger.Debug("Network status changed.");
			Update();
		}

		private bool TryGetCredentials(string network, out PasswordCredential credentials)
		{
			var args = new CredentialsRequiredEventArgs { NetworkName = network };

			credentials = new PasswordCredential();

			CredentialsRequired?.Invoke(args);

			if (args.Success)
			{
				if (!string.IsNullOrEmpty(args.Password))
				{
					credentials.Password = args.Password;
				}

				if (!string.IsNullOrEmpty(args.Username))
				{
					credentials.UserName = args.Username;
				}
			}

			return args.Success;
		}

		private bool TryGetCurrentWirelessNetwork(out string name)
		{
			name = default;

			if (HasWirelessAdapter)
			{
				try
				{
					var getProfile = adapter.NetworkAdapter.GetConnectedProfileAsync().AsTask();
					var profile = getProfile.GetAwaiter().GetResult();

					if (profile?.IsWlanConnectionProfile == true)
					{
						name = profile.WlanConnectionProfileDetails.GetConnectedSsid();
					}
				}
				catch
				{
				}
			}

			return name != default;
		}

		private void Update(bool rescan = true)
		{
			try
			{
				var currentNetwork = default(WirelessNetwork);
				var hasConnection = nativeMethods.HasInternetConnection();
				var isConnecting = Status == ConnectionStatus.Connecting;
				var networks = new List<WirelessNetwork>();
				var previousStatus = Status;

				if (HasWirelessAdapter)
				{
					hasConnection &= TryGetCurrentWirelessNetwork(out var current);

					foreach (var network in adapter.NetworkReport.AvailableNetworks.FilterAndOrder())
					{
						var wirelessNetwork = network.ToWirelessNetwork();

						if (network.Ssid == current)
						{
							currentNetwork = wirelessNetwork;
							wirelessNetwork.Status = ConnectionStatus.Connected;
						}

						networks.Add(wirelessNetwork);
					}

					if (rescan)
					{
						_ = adapter.ScanAsync();
					}
				}

				lock (@lock)
				{
					wirelessNetworks.Clear();
					wirelessNetworks.AddRange(networks);
				}

				Type = HasWirelessAdapter ? ConnectionType.Wireless : (hasConnection ? ConnectionType.Wired : ConnectionType.Undefined);
				Status = hasConnection ? ConnectionStatus.Connected : (isConnecting ? ConnectionStatus.Connecting : ConnectionStatus.Disconnected);

				LogNetworkChanges(previousStatus, currentNetwork);
			}
			catch (Exception e)
			{
				logger.Error("Failed to update network adapter!", e);
			}

			Changed?.Invoke();
		}

		private void LogNetworkChanges(ConnectionStatus previousStatus, WirelessNetwork currentNetwork = default)
		{
			if (previousStatus != ConnectionStatus.Connected && Status == ConnectionStatus.Connected)
			{
				logger.Info($"Connection established ({Type}{(currentNetwork != default ? $", {currentNetwork.Name}" : "")}).");
			}

			if (previousStatus != ConnectionStatus.Disconnected && Status == ConnectionStatus.Disconnected)
			{
				logger.Info("Connection lost.");
			}
		}
	}
}
