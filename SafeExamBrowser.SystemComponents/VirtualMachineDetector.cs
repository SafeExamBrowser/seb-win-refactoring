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

namespace SafeExamBrowser.SystemComponents
{
	public class VirtualMachineDetector : IVirtualMachineDetector
	{
		private const string QEMU_MAC_PREFIX = "525400";
		private const string VIRTUALBOX_MAC_PREFIX = "080027";

		private static readonly string[] DeviceBlacklist =
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

		private static readonly string[] DeviceWhitelist =
		{
			// Microsoft Virtual Disk Device
			"PROD_VIRTUAL_DISK",
			// Microsoft Virtual DVD Device
			"PROD_VIRTUAL_DVD"
		};

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
			var devices = systemInfo.PlugAndPlayDeviceIds;
			var isVirtualMachine = false;
			var macAddress = systemInfo.MacAddress;
			var manufacturer = systemInfo.Manufacturer;
			var model = systemInfo.Model;

			isVirtualMachine |= IsVirtualRegistry();
			isVirtualMachine |= IsVirtualSystemInfo(biosInfo, manufacturer, model);
			isVirtualMachine |= IsVirtualWmi();

			if (macAddress != null && macAddress.Count() > 2)
			{
				isVirtualMachine |= macAddress.StartsWith(QEMU_MAC_PREFIX);
				isVirtualMachine |= macAddress.StartsWith(VIRTUALBOX_MAC_PREFIX);
				isVirtualMachine |= macAddress.StartsWith("000000000000");
			}

			foreach (var device in devices)
			{
				isVirtualMachine |= DeviceBlacklist.Any(d => device.ToLower().Contains(d.ToLower())) && DeviceWhitelist.All(d => !device.ToLower().Contains(d.ToLower()));
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
			isVirtualMachine |= biosInfo.Contains("edk ii unknown");
			isVirtualMachine |= manufacturer.Contains("microsoft corporation") && !model.Contains("surface");
			isVirtualMachine |= manufacturer.Contains("parallels software");
			isVirtualMachine |= manufacturer.Contains("qemu");
			isVirtualMachine |= manufacturer.Contains("vmware");
			isVirtualMachine |= model.Contains("virtualbox");
			isVirtualMachine |= model.Contains("Q35 +");

			return isVirtualMachine;
		}

		private bool IsVirtualRegistry()
		{
			var isVirtualMachine = false;

			isVirtualMachine |= HasHistoricVirtualMachineHardwareConfiguration();
			isVirtualMachine |= HasLocalVirtualMachineDeviceCache();

			return isVirtualMachine;
		}

		private bool HasHistoricVirtualMachineHardwareConfiguration()
		{
			/** 
			 * scanned registry format:
			 * 
			 * HKLM\SYSTEM\HardwareConfig\{configId=uuid}
			 *	 - BIOSVendor
			 *	 - SystemManufacturer
			 *	 - ...
			 *	 \ComputerIds
			 *	   - {computerId=uuid}: {computerSummary=hardwareInfo}
			 * 
			 */
			const string HARDWARE_ROOT_KEY = "HKEY_LOCAL_MACHINE\\SYSTEM\\HardwareConfig";

			var isVirtualMachine = false;

			if (!registry.TryGetSubKeys(HARDWARE_ROOT_KEY, out var hardwareConfigSubkeys))
			{
				return false;
			}

			foreach (var configId in hardwareConfigSubkeys)
			{
				var hardwareConfigKey = $"{HARDWARE_ROOT_KEY}\\{configId}";
				var didReadKeys = true;

				didReadKeys &= registry.TryRead(hardwareConfigKey, "BIOSVendor", out var biosVendor);
				didReadKeys &= registry.TryRead(hardwareConfigKey, "BIOSVersion", out var biosVersion);
				didReadKeys &= registry.TryRead(hardwareConfigKey, "SystemManufacturer", out var systemManufacturer);
				didReadKeys &= registry.TryRead(hardwareConfigKey, "SystemProductName", out var systemProductName);

				if (!didReadKeys)
				{
					continue;
				}

				var biosInfo = $"{(string) biosVendor} {(string) biosVersion}";

				isVirtualMachine |= IsVirtualSystemInfo(biosInfo, (string) systemManufacturer, (string) systemProductName);

				var computerIdsKey = $"{hardwareConfigKey}\\ComputerIds";
				if (!registry.TryGetNames(computerIdsKey, out var computerIdNames))
				{
					continue;
				}

				foreach (var computerIdName in computerIdNames)
				{
					if (!registry.TryRead(computerIdsKey, computerIdName, out var computerSummary))
					{
						continue;
					}

					isVirtualMachine |= IsVirtualSystemInfo((string) computerSummary, (string) systemManufacturer, (string) systemProductName);
				}
			}

			return isVirtualMachine;
		}

		private bool HasLocalVirtualMachineDeviceCache()
		{
			const string DEVICE_CACHE_PARENT_KEY = "HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\TaskFlow\\DeviceCache";

			var isVirtualMachine = false;
			var deviceName = System.Environment.GetEnvironmentVariable("COMPUTERNAME");
			var hasDeviceCacheKeys = registry.TryGetSubKeys(DEVICE_CACHE_PARENT_KEY, out var deviceCacheKeys);

			if (deviceName != null && hasDeviceCacheKeys)
			{
				foreach (var cacheId in deviceCacheKeys)
				{
					var cacheIdKey = $"{DEVICE_CACHE_PARENT_KEY}\\{cacheId}";
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
				isVirtualMachine |= ((string) cpuObj["Name"]).ToLower().Contains(" kvm ");
			}

			return isVirtualMachine;
		}
	}
}
