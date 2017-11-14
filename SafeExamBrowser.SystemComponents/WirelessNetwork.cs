/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Timers;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.SystemComponents;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;
using SimpleWifi;
using SimpleWifi.Win32;
using SimpleWifi.Win32.Interop;

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
