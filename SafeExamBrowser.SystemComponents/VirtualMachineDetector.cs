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
using SafeExamBrowser.SystemComponents.Contracts.Registry;
using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using System;

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
		private readonly IRegistry registry;
		private readonly ISystemInfo systemInfo;

		public VirtualMachineDetector(ILogger logger, IRegistry registry, ISystemInfo systemInfo)
		{
			this.logger = logger;
			this.registry = registry; 
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

			/** 
			 * check historic hardware profiles
			 * 
			 * HKLM\SYSTEM\HardwareConfig\{configId=uuid}\ComputerIds
			 *	- {computerId=uuid}: {computerSummary=hardwareInfo}
			 *	
			 */
			IEnumerable<string> hardwareConfigSubkeys;
			if (!registry.TryGetSubKeys("HKLM\\SYSTEM\\HardwareConfig", out hardwareConfigSubkeys))
				return false;

			foreach (string configId in hardwareConfigSubkeys)
			{
				logger.Info($"scanning configId: {configId}");
				var configKey = $"HKEY_LOCAL_MACHINE\\SYSTEM\\HardwareConfig\\{configId}";

				object biosVendor;
				object biosVersion;
				object systemManufacturer;
				object systemProductName;

				bool success = true;

				success &= registry.TryRead(configKey, "BIOSVendor", out biosVendor);
				success &= registry.TryRead(configKey, "BIOSVersion", out biosVersion);
				success &= registry.TryRead(configKey, "SystemManufacturer", out systemManufacturer);
				success &= registry.TryRead(configKey, "SystemProductName", out systemProductName);

				if (!success)
					continue;

				// reconstruct the systemInfo.biosInfo string
				string biosInfo = $"{(string) biosVendor} {(string) biosVersion}";

				isVirtualMachine |= IsVirtualSystemInfo(biosInfo, (string) systemManufacturer, (string) systemProductName);

				// hardware information of profile throughout installation etc. 
				IEnumerable<string> computerIds;
				if (!registry.TryGetSubKeys($"HKLM\\SYSTEM\\HardwareConfig\\{configId}\\ComputerIds", out computerIds))
					return false;

				foreach (var computerId in computerIds)
				{
					logger.Info($"computerId: {computerId}");
					// e.g. manufacturer&version&sku&...
					object computerSummary; // = (string) computerIds.GetValue(computerId);

					if (!registry.TryRead($"HKLM\\SYSTEM\\HardwareConfig\\{configId}\\ComputerIds", computerId, out computerSummary))
						continue;

					isVirtualMachine |= IsVirtualSystemInfo((string) computerSummary, (string) systemManufacturer, (string) systemProductName);
				}
			}

			// check Windows timeline caches for current hardware config
			/*IEnumerable<string> deviceCacheSubkeys;
			if (registry.TryGetSubKeys($"HKCU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\TaskFlow\\DeviceCache", out deviceCacheSubkeys)
			{
				foreach (string deviceCacheKey in deviceCacheSubkeys)
				{
					if (registry.TryRead($"HKCU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\TaskFlow\\DeviceCache"))*/


			var deviceCacheKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey($"HKCU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\TaskFlow\\DeviceCache");
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
					if (currHostname.ToLower() == cacheHostname)
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
