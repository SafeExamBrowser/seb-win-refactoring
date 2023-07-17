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

		/// <summary>
		/// Scans parameters for disallowed strings (signatures)
		/// </summary>
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

			// the resulting IsVirtualRegistry() would be massive so split it
			isVirtualMachine |= IsVirtualRegistryHardwareConfig();
			isVirtualMachine |= IsVirtualRegistryDeviceCache();

			return isVirtualMachine;
		}


		/// <summary>
		/// Scans (historic) hardware configurations in the registry.
		/// </summary>
		private bool IsVirtualRegistryHardwareConfig()
		{
			bool isVirtualMachine = false;

			/** 
			 * scanned registry format:
			 * 
			 * HKLM\SYSTEM\HardwareConfig\{configId=uuid}\ComputerIds
			 *	- {computerId=uuid}: {computerSummary=hardwareInfo}
			 *	
			 */
			IEnumerable<string> hardwareConfigSubkeys;
			const string hwConfigParentKey = "HKEY_LOCAL_MACHINE\\SYSTEM\\HardwareConfig";
			if (!registry.TryGetSubKeys(hwConfigParentKey, out hardwareConfigSubkeys))
				return false;

			foreach (string configId in hardwareConfigSubkeys)
			{
				logger.Info($"scanning configId: {configId}");
				var hwConfigKey = $"{hwConfigParentKey}\\{configId}";

				// collect system values for IsVirtualSystemInfo()
				object biosVendor;
				object biosVersion;
				object systemManufacturer;
				object systemProductName;

				bool success = true;
				success &= registry.TryRead(hwConfigKey, "BIOSVendor", out biosVendor);
				success &= registry.TryRead(hwConfigKey, "BIOSVersion", out biosVersion);
				success &= registry.TryRead(hwConfigKey, "SystemManufacturer", out systemManufacturer);
				success &= registry.TryRead(hwConfigKey, "SystemProductName", out systemProductName);

				if (!success)
					continue;

				// reconstruct the systemInfo.biosInfo string
				string biosInfo = $"{(string) biosVendor} {(string) biosVersion}";

				isVirtualMachine |= IsVirtualSystemInfo(biosInfo, (string) systemManufacturer, (string) systemProductName);

				// check even more hardware information 
				IEnumerable<string> computerIdNames;
				var computerIdsKey = $"{hwConfigKey}\\ComputerIds";
				if (!registry.TryGetNames(computerIdsKey, out computerIdNames))
					continue;

				foreach (var computerIdName in computerIdNames)
				{
					logger.Info($"computerId: {computerIdName}");

					// collect computer hardware summary (e.g. manufacturer&version&sku&...)
					object computerSummary;
					if (!registry.TryRead(computerIdsKey, computerIdName, out computerSummary))
						continue;

					isVirtualMachine |= IsVirtualSystemInfo((string) computerSummary, (string) systemManufacturer, (string) systemProductName);
				}
			}

			return isVirtualMachine;
		}

		/// <summary>
		/// Scans (synced) device cache for hardware info of the current device.
		/// </summary>
		private bool IsVirtualRegistryDeviceCache()
		{
			bool isVirtualMachine = false;

			// device cache contains hardware about other devices logged into as well, so lock onto this device in case an innocent VM was logged into.
			// in the future, try to improve this check somehow since DeviceCache only gives ComputerName
			var deviceName = System.Environment.GetEnvironmentVariable("COMPUTERNAME");

			// check Windows timeline caches for current hardware config
			const string deviceCacheParentKey = "HKCU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\TaskFlow\\DeviceCache";
			IEnumerable<string> deviceCacheKeys;
			bool has_dc_keys = registry.TryGetSubKeys(deviceCacheParentKey, out deviceCacheKeys);

			if (deviceName != null && has_dc_keys)
			{
				foreach (string cacheId in deviceCacheKeys)
				{
					var cacheIdKey = $"{deviceCacheParentKey}\\{cacheId}";
					object cacheDeviceName;
					object cacheDeviceManufacturer;
					object cacheDeviceModel;

					bool success = true;
					success &= registry.TryRead(cacheIdKey, "DeviceName", out cacheDeviceName);

					if (!success || deviceName.ToLower() != ((string) cacheDeviceName).ToLower())
						continue;

					success &= registry.TryRead(cacheIdKey, "DeviceMake", out cacheDeviceManufacturer);
					success &= registry.TryRead(cacheIdKey, "DeviceModel", out cacheDeviceModel);
					if (!success)
						continue;

					isVirtualMachine |= IsVirtualSystemInfo("", (string) cacheDeviceManufacturer, (string) cacheDeviceModel);
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
