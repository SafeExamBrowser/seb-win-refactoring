/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Timers;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.SystemComponents;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;
using SimpleWifi;
using SimpleWifi.Win32;

namespace SafeExamBrowser.SystemComponents
{
	public class WirelessNetwork : ISystemComponent<ISystemWirelessNetworkControl>
	{
		private const int TWO_SECONDS = 2000;

		private ILogger logger;
		private ISystemWirelessNetworkControl control;
		private Timer timer;
		private Wifi wifi;

		public WirelessNetwork(ILogger logger)
		{
			this.logger = logger;
		}

		public void Initialize(ISystemWirelessNetworkControl control)
		{
			this.control = control;
			this.wifi = new Wifi();

			if (wifi.NoWifiAvailable || IsTurnedOff())
			{
				control.HasWirelessNetworkAdapter = false;
				logger.Info("Wireless networks cannot be monitored, as there is no hardware adapter available or it is turned off.");
			}
			else
			{
				control.HasWirelessNetworkAdapter = true;
				control.NetworkSelected += Control_NetworkSelected;
				wifi.ConnectionStatusChanged += Wifi_ConnectionStatusChanged;

				UpdateControl();

				timer = new Timer(TWO_SECONDS);
				timer.Elapsed += Timer_Elapsed;
				timer.AutoReset = true;
				timer.Start();

				logger.Info("Started monitoring the wireless network adapter.");
			}
		}

		public void Terminate()
		{
			timer?.Stop();
			control?.Close();

			if (timer != null)
			{
				logger.Info("Stopped monitoring the wireless network adapter.");
			}
		}

		private void Control_NetworkSelected(IWirelessNetwork network)
		{
			throw new NotImplementedException();
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			UpdateControl();
		}

		private void Wifi_ConnectionStatusChanged(object sender, WifiStatusEventArgs e)
		{
			control.NetworkStatus = ToStatus(e.NewStatus);
		}

		private bool IsTurnedOff()
		{
			try
			{
				// See https://msdn.microsoft.com/en-us/library/aa394216(v=vs.85).aspx
				string query = @"
					SELECT *
					FROM Win32_NetworkAdapter";
				var searcher = new ManagementObjectSearcher(query);
				var adapters = searcher.Get();
				var interfaces = NetworkInterface.GetAllNetworkInterfaces().Where(i => i.NetworkInterfaceType == NetworkInterfaceType.Wireless80211).ToList();

				logger.Info("Interface count: " + interfaces.Count);

				foreach (var i in interfaces)
				{
					logger.Info(i.Description);
					logger.Info(i.Id);
					logger.Info(i.Name);
					logger.Info(i.NetworkInterfaceType.ToString());
					logger.Info(i.OperationalStatus.ToString());
					logger.Info("-----");
				}

				foreach (var adapter in adapters)
				{
					logger.Info("-------");
					
					foreach (var property in adapter.Properties)
					{
						logger.Info($"{property.Name}: {property.Value} ({property.Type})");
					}
				}

				logger.Info("Adapter count: " + adapters.Count);

				return true;

				using (var client = new WlanClient())
				{
					foreach (var @interface in client.Interfaces)
					{
						Trace.WriteLine($"[{@interface.InterfaceName}]");

						foreach (var state in @interface.RadioState.PhyRadioState)
						{
							Trace.WriteLine($"PhyIndex: {state.dwPhyIndex}");
							Trace.WriteLine($"SoftwareRadioState: {state.dot11SoftwareRadioState}");
							Trace.WriteLine($"HardwareRadioState: {state.dot11HardwareRadioState}");
						}
					}
				}
			}
			catch (Exception e)
			{
				logger.Error("Fail!", e);

				return true;
			}
		}

		private void UpdateControl()
		{
			var networks = new List<IWirelessNetwork>();

			control.NetworkStatus = ToStatus(wifi.ConnectionStatus);

			try
			{
				foreach (var accessPoint in wifi.GetAccessPoints())
				{
					// The user may only connect to an already configured wireless network!
					if (accessPoint.HasProfile)
					{
						networks.Add(new WirelessNetworkDefinition
						{
							Name = accessPoint.Name,
							SignalStrength = Convert.ToInt32(accessPoint.SignalStrength),
							Status = accessPoint.IsConnected ? WirelessNetworkStatus.Connected : WirelessNetworkStatus.Disconnected
						});
					}
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to update the wireless network adapter status!", e);
			}

			control.Update(networks);
		}

		private WirelessNetworkStatus ToStatus(WifiStatus status)
		{
			return status == WifiStatus.Connected ? WirelessNetworkStatus.Connected : WirelessNetworkStatus.Disconnected;
		}
	}
}
