/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
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
using SafeExamBrowser.Contracts.UserInterface.Taskbar;
using SimpleWifi;
using SimpleWifi.Win32;
using SimpleWifi.Win32.Interop;

namespace SafeExamBrowser.SystemComponents
{
	public class WirelessNetwork : ISystemComponent<ISystemWirelessNetworkControl>
	{
		private const int TWO_SECONDS = 2000;
		private readonly object @lock = new object();

		private ISystemWirelessNetworkControl control;
		private ILogger logger;
		private IList<WirelessNetworkDefinition> networks = new List<WirelessNetworkDefinition>();
		private IText text;
		private Timer timer;
		private Wifi wifi;

		public WirelessNetwork(ILogger logger, IText text)
		{
			this.logger = logger;
			this.text = text;
		}

		public void Initialize(ISystemWirelessNetworkControl control)
		{
			this.control = control;
			this.wifi = new Wifi();

			if (wifi.NoWifiAvailable || IsTurnedOff())
			{
				control.HasWirelessNetworkAdapter = false;
				control.SetTooltip(text.Get(TextKey.SystemControl_WirelessNotAvailable));
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
			try
			{
				var accessPoint = networks.First(n => n.Id == network.Id).AccessPoint;
				var authRequest = new AuthRequest(accessPoint);

				accessPoint.ConnectAsync(authRequest, false, (success) => AccessPoint_OnConnectComplete(network.Name, success));
				control.IsConnecting = true;
			}
			catch (Exception e)
			{
				logger.Error($"Failed to connect to wireless network '{network.Name}!'", e);
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

			control.IsConnecting = false;
			UpdateControl();
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			UpdateControl();
		}

		private void Wifi_ConnectionStatusChanged(object sender, WifiStatusEventArgs e)
		{
			UpdateControl();
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
			lock (@lock)
			{
				try
				{
					networks.Clear();

					foreach (var accessPoint in wifi.GetAccessPoints())
					{
						// The user may only connect to an already configured wireless network!
						if (accessPoint.HasProfile)
						{
							networks.Add(ToDefinition(accessPoint));

							if (accessPoint.IsConnected)
							{
								control.SetTooltip(text.Get(TextKey.SystemControl_WirelessConnected).Replace("%%NAME%%", accessPoint.Name));
							}
						}
					}

					if (wifi.ConnectionStatus == WifiStatus.Disconnected)
					{
						control.SetTooltip(text.Get(TextKey.SystemControl_WirelessDisconnected));
					}

					control.NetworkStatus = ToStatus(wifi.ConnectionStatus);
					control.Update(new List<IWirelessNetwork>(networks));
				}
				catch (Exception e)
				{
					logger.Error("Failed to update the wireless network adapter status!", e);
				}
			}
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
