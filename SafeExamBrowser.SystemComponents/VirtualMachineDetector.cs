/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Linq;
using System.Management;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts;
using Microsoft.Win32;

namespace SafeExamBrowser.SystemComponents
{
	public class VirtualMachineDetector : IVirtualMachineDetector
	{
		private static readonly string[] DEVICE_BLACKLIST =
		{
			// Hyper-V
			"PROD_VIRTUAL", "HYPER_V",
			// QEMU
			"qemu", "ven_1af4", "ven_1b36", "subsys_11001af4",
			// VirtualBox
			"vbox", "vid_80ee",
			// VMware
			"PROD_VMWARE", "VEN_VMWARE", "VMWARE_IDE"
		};
		private static readonly string QEMU_MAC_PREFIX = "525400";
		private static readonly string VIRTUALBOX_MAC_PREFIX = "080027";

		private readonly ILogger logger;
		private readonly ISystemInfo systemInfo;

		public VirtualMachineDetector(ILogger logger, ISystemInfo systemInfo)
		{
			this.logger = logger;
			this.systemInfo = systemInfo;
		}

		public bool IsVirtualMachine()
		{
			var biosInfo = systemInfo.BiosInfo;
			var isVirtualMachine = false;
			var macAddress = systemInfo.MacAddress;
			var manufacturer = systemInfo.Manufacturer;
			var model = systemInfo.Model;
			var devices = systemInfo.PlugAndPlayDeviceIds;

			// redundancy: registry check does this aswell (systemInfo may be using different methods)
			isVirtualMachine |= IsVirtualSystemInfo(biosInfo, manufacturer, model);
			isVirtualMachine |= IsVirtualWmi();
			isVirtualMachine |= IsVirtualRegistry();

			if (macAddress != null && macAddress.Count() > 2)
			{
				isVirtualMachine |= macAddress.StartsWith(QEMU_MAC_PREFIX);
				isVirtualMachine |= macAddress.StartsWith(VIRTUALBOX_MAC_PREFIX);
				isVirtualMachine |= macAddress.StartsWith("000000000000");  // indicates tampering
			}

			foreach (var device in devices)
			{
				isVirtualMachine |= DEVICE_BLACKLIST.Any(d => device.ToLower().Contains(d.ToLower()));
			}

			logger.Debug($"Computer '{systemInfo.Name}' appears {(isVirtualMachine ? "" : "not ")}to be a virtual machine.");

			return isVirtualMachine;
		}

		private bool IsVirtualSystemInfo(string biosInfo, string manufacturer, string model)
		{
			var isVirtualMachine = false;

			biosInfo = biosInfo.ToLower();
			manufacturer = manufacturer.ToLower();
			model = model.ToLower();

			isVirtualMachine |= biosInfo.Contains("hyper-v");
			isVirtualMachine |= biosInfo.Contains("virtualbox");
			isVirtualMachine |= biosInfo.Contains("vmware");
			isVirtualMachine |= biosInfo.Contains("ovmf");
			isVirtualMachine |= biosInfo.Contains("edk ii unknown");  // qemu
			isVirtualMachine |= manufacturer.Contains("microsoft corporation") && !model.Contains("surface");
			isVirtualMachine |= manufacturer.Contains("parallels software");
			isVirtualMachine |= manufacturer.Contains("qemu");
			isVirtualMachine |= manufacturer.Contains("vmware");
			isVirtualMachine |= model.Contains("virtualbox");
			isVirtualMachine |= model.Contains("Q35 +");  // qemu

			return isVirtualMachine;
		}

		private bool IsVirtualRegistry()
		{
			var isVirtualMachine = false;

			// check historic hardware profiles
			var hardwareConfKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SYSTEM\\HardwareConfig");
			if (hardwareConfKey != null)
			{
				foreach (string configId in hardwareConfKey.GetSubKeyNames())
				{
					var configKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey($"SYSTEM\\HardwareConfig\\{configId}");

					if (configKey == null)
					{
						continue;
					}

					// reconstruct the systemInfo.biosInfo string
					var biosInfo = (string) configKey.GetValue("BIOSVendor") + " " + (string) configKey.GetValue("BIOSVersion");
					var manufacturer = (string) configKey.GetValue("SystemManufacturer");
					var model = (string) configKey.GetValue("SystemProductName");

					isVirtualMachine |= IsVirtualSystemInfo(biosInfo, manufacturer, model);

					// hardware information of profile throughout installation etc. 
					var computerIdsKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey($"SYSTEM\\HardwareConfig\\{configId}\\ComputerIds");

					if (computerIdsKey == null)
					{
						continue;
					}

					foreach (var computerId in computerIdsKey.GetSubKeyNames())
					{
						// e.g. manufacturer&version&sku&...
						var computerSummary = (string) computerIdsKey.GetValue(computerId);
						isVirtualMachine |= IsVirtualSystemInfo(computerSummary, computerSummary, computerSummary);
					}
				}
			}

			// check Windows timeline caches for current hardware config
			var deviceCacheKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey($"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\TaskFlow\\DeviceCache");
			var currHostname = System.Environment.GetEnvironmentVariable("COMPUTERNAME");

			if (deviceCacheKey != null && currHostname != null)
			{
				foreach (var cacheId in deviceCacheKey.GetSubKeyNames())
				{
					var cacheKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey($"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\TaskFlow\\DeviceCache\\{cacheId}");

					if (cacheKey == null)
					{
						continue;
					}

					var cacheHostname = ((string) cacheKey.GetValue("DeviceName")).ToLower();

					// windows timeline syncs with other hosts that a user has logged into: check hostname to only check this device
					if (cacheHostname == currHostname)
					{
						var biosInfo = "";
						var manufacturer = (string) cacheKey.GetValue("DeviceMake");
						var model = (string) cacheKey.GetValue("DeviceModel");

						isVirtualMachine |= IsVirtualSystemInfo(biosInfo, manufacturer, model);
					}
				}
			}

			return isVirtualMachine;
		}

		private bool IsVirtualWmi()
		{
			var isVirtualMachine = false;
			var cpuObjSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");

			foreach (var cpuObj in cpuObjSearcher.Get())
			{
				isVirtualMachine |= ((string) cpuObj["Name"]).ToLower().Contains(" kvm ");  // qemu (KVM specifically)
			}

			return isVirtualMachine;
		}
	}
}
