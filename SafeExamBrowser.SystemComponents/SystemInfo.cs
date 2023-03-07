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
using System.Management;
using System.Windows.Forms;
using SafeExamBrowser.SystemComponents.Contracts;
using BatteryChargeStatus = System.Windows.Forms.BatteryChargeStatus;
using OperatingSystem = SafeExamBrowser.SystemComponents.Contracts.OperatingSystem;

namespace SafeExamBrowser.SystemComponents
{
	public class SystemInfo : ISystemInfo
	{
		public string BiosInfo { get; private set; }
		public bool HasBattery { get; private set; }
		public string MacAddress { get; private set; }
		public string Manufacturer { get; private set; }
		public string Model { get; private set; }
		public string Name { get; private set; }
		public OperatingSystem OperatingSystem { get; private set; }
		public string OperatingSystemInfo => $"{OperatingSystemName()}, {Environment.OSVersion.VersionString} ({Architecture()})";
		public string[] PlugAndPlayDeviceIds { get; private set; }

		public SystemInfo()
		{
			InitializeBattery();
			InitializeBiosInfo();
			InitializeMacAddress();
			InitializeMachineInfo();
			InitializeOperatingSystem();
			InitializePnPDevices();
		}

		private void InitializeBattery()
		{
			var status = SystemInformation.PowerStatus.BatteryChargeStatus;

			HasBattery = !status.HasFlag(BatteryChargeStatus.NoSystemBattery);
			HasBattery &= !status.HasFlag(BatteryChargeStatus.Unknown);
		}

		private void InitializeBiosInfo()
		{
			var manufacturer = default(string);
			var name = default(string);

			try
			{
				using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS"))
				using (var results = searcher.Get())
				using (var bios = results.Cast<ManagementObject>().First())
				{
					foreach (var property in bios.Properties)
					{
						if (property.Name.Equals("Manufacturer"))
						{
							manufacturer = Convert.ToString(property.Value);
						}
						else if (property.Name.Equals("Name"))
						{
							name = Convert.ToString(property.Value);
						}
					}
				}

				BiosInfo = $"{manufacturer} {name}";
			}
			catch (Exception)
			{
				BiosInfo = "";
			}
		}

		private void InitializeMachineInfo()
		{
			var model = default(string);
			var systemFamily = default(string);

			try
			{
				using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
				using (var results = searcher.Get())
				using (var system = results.Cast<ManagementObject>().First())
				{
					foreach (var property in system.Properties)
					{
						if (property.Name.Equals("Manufacturer"))
						{
							Manufacturer = Convert.ToString(property.Value);
						}
						else if (property.Name.Equals("Model"))
						{
							model = Convert.ToString(property.Value);
						}
						else if (property.Name.Equals("Name"))
						{
							Name = Convert.ToString(property.Value);
						}
						else if (property.Name.Equals("SystemFamily"))
						{
							systemFamily = Convert.ToString(property.Value);
						}
					}
				}

				Model = $"{systemFamily} {model}";
			}
			catch (Exception)
			{
				Manufacturer = "";
				Model = "";
				Name = "";
			}
		}

		private void InitializeOperatingSystem()
		{
			// IMPORTANT:
			// In order to be able to retrieve the correct operating system version via System.Environment.OSVersion,
			// the executing assembly needs to define an application manifest specifying all supported Windows versions!
			var major = Environment.OSVersion.Version.Major;
			var minor = Environment.OSVersion.Version.Minor;
			var build = Environment.OSVersion.Version.Build;

			// See https://en.wikipedia.org/wiki/List_of_Microsoft_Windows_versions for mapping source...
			if (major == 6)
			{
				if (minor == 1)
				{
					OperatingSystem = OperatingSystem.Windows7;
				}
				else if (minor == 2)
				{
					OperatingSystem = OperatingSystem.Windows8;
				}
				else if (minor == 3)
				{
					OperatingSystem = OperatingSystem.Windows8_1;
				}
			}
			else if (major == 10)
			{
				if (build < 22000)
				{
					OperatingSystem = OperatingSystem.Windows10;
				}
				else
				{
					OperatingSystem = OperatingSystem.Windows11;
				}
			}
		}

		private string OperatingSystemName()
		{
			switch (OperatingSystem)
			{
				case OperatingSystem.Windows7:
					return "Windows 7";
				case OperatingSystem.Windows8:
					return "Windows 8";
				case OperatingSystem.Windows8_1:
					return "Windows 8.1";
				case OperatingSystem.Windows10:
					return "Windows 10";
				case OperatingSystem.Windows11:
					return "Windows 11";
				default:
					return "Unknown Windows Version";
			}
		}

		private string Architecture()
		{
			return Environment.Is64BitOperatingSystem ? "x64" : "x86";
		}

		private void InitializeMacAddress()
		{
			const string UNDEFINED = "000000000000";

			try
			{
				using (var searcher = new ManagementObjectSearcher("SELECT MACAddress FROM Win32_NetworkAdapterConfiguration WHERE DNSDomain IS NOT NULL"))
				using (var results = searcher.Get())
				{
					if (results != null && results.Count > 0)
					{
						using (var networkAdapter = results.Cast<ManagementObject>().First())
						{
							foreach (var property in networkAdapter.Properties)
							{
								if (property.Name.Equals("MACAddress"))
								{
									MacAddress = Convert.ToString(property.Value).Replace(":", "").ToUpper();
								}
							}
						}
					}
					else
					{
						MacAddress = UNDEFINED;
					}
				}
			}
			catch (Exception)
			{
				MacAddress = UNDEFINED;
			}
		}

		private void InitializePnPDevices()
		{
			var deviceList = new List<string>();

			try
			{
				using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT DeviceID FROM Win32_PnPEntity"))
				using (var results = searcher.Get())
				{
					foreach (ManagementObject queryObj in results)
					{
						using (queryObj)
						{
							foreach (var property in queryObj.Properties)
							{
								if (property.Name.Equals("DeviceID"))
								{
									deviceList.Add(Convert.ToString(property.Value).ToLower());
								}
							}
						}
					}
				}
			}
			finally
			{
				PlugAndPlayDeviceIds = deviceList.ToArray();
			}
		}
	}
}
