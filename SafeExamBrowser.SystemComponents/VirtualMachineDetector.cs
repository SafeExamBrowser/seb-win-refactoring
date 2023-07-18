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

			// the resulting IsVirtualRegistry() would be massive so split it
			isVirtualMachine |= HasHistoricVirtualMachineHardwareConfiguration();
			isVirtualMachine |= HasLocalVirtualMachineDeviceCache();

			return isVirtualMachine;
		}

		private bool HasHistoricVirtualMachineHardwareConfiguration()
		{
			var isVirtualMachine = false;

			/** 
			 * scanned registry format:
			 * 
			 * HKLM\SYSTEM\HardwareConfig\{configId=uuid}\ComputerIds
			 *	- {computerId=uuid}: {computerSummary=hardwareInfo}
			 *	
			 */
			const string hwConfigParentKey = "HKEY_LOCAL_MACHINE\\SYSTEM\\HardwareConfig";
			if (!registry.TryGetSubKeys(hwConfigParentKey, out var hardwareConfigSubkeys))
			{
				return false;
			}

			foreach (var configId in hardwareConfigSubkeys)
			{
				var hwConfigKey = $"{hwConfigParentKey}\\{configId}";
				var didReadKeys = true;

				// collect system values for IsVirtualSystemInfo()
				didReadKeys &= registry.TryRead(hwConfigKey, "BIOSVendor", out var biosVendor);
				didReadKeys &= registry.TryRead(hwConfigKey, "BIOSVersion", out var biosVersion);
				didReadKeys &= registry.TryRead(hwConfigKey, "SystemManufacturer", out var systemManufacturer);
				didReadKeys &= registry.TryRead(hwConfigKey, "SystemProductName", out var systemProductName);
				if (!didReadKeys)
				{
					continue;
				}

				// reconstruct the systemInfo.biosInfo string
				var biosInfo = $"{(string) biosVendor} {(string) biosVersion}";

				isVirtualMachine |= IsVirtualSystemInfo(biosInfo, (string) systemManufacturer, (string) systemProductName);

				// check even more hardware information 
				var computerIdsKey = $"{hwConfigKey}\\ComputerIds";
				if (!registry.TryGetNames(computerIdsKey, out var computerIdNames))
				{
					continue;
				}

				foreach (var computerIdName in computerIdNames)
				{
					// collect computer hardware summary (e.g. manufacturer&version&sku&...)
					if (!registry.TryRead(computerIdsKey, computerIdName, out var computerSummary))
					{
						continue;
					}

					isVirtualMachine |= IsVirtualSystemInfo((string) computerSummary, (string) systemManufacturer, (string) systemProductName);
				}
			}

			return isVirtualMachine;
		}

		/// <summary>
		/// Scans (synced) device cache for hardware info of the current device.
		/// </summary>
		private bool HasLocalVirtualMachineDeviceCache()
		{
			var isVirtualMachine = false;

			// device cache contains hardware about other devices logged into as well, so lock onto this device in case an innocent VM was logged into.
			// in the future, try to improve this check somehow since DeviceCache only gives ComputerName
			var deviceName = System.Environment.GetEnvironmentVariable("COMPUTERNAME");

			// check Windows timeline caches for current hardware config
			const string deviceCacheParentKey = "HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\TaskFlow\\DeviceCache";
			var hasDeviceCacheKeys = registry.TryGetSubKeys(deviceCacheParentKey, out var deviceCacheKeys);

			if (deviceName != null && hasDeviceCacheKeys)
			{
				foreach (var cacheId in deviceCacheKeys)
				{
					var cacheIdKey = $"{deviceCacheParentKey}\\{cacheId}";
					var didReadKeys = true;

					didReadKeys &= registry.TryRead(cacheIdKey, "DeviceName", out var cacheDeviceName);
					if (!didReadKeys || deviceName.ToLower() != ((string) cacheDeviceName).ToLower())
					{
						continue;
					}

					didReadKeys &= registry.TryRead(cacheIdKey, "DeviceMake", out var cacheDeviceManufacturer);
					didReadKeys &= registry.TryRead(cacheIdKey, "DeviceModel", out var cacheDeviceModel);
					if (!didReadKeys)
					{
						continue;
					}

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
