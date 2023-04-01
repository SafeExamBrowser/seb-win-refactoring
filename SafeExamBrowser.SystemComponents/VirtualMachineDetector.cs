/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
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
		private bool IsVirtualSystemInfo(string biosInfo, string manufacturer, string model)
		{
			bool isVirtualMachine = false;

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
			isVirtualMachine |= model.Contains("Q35 +");

			return isVirtualMachine;
		}

		private bool IsVirtualRegistry()
		{
			bool isVirtualMachine = false;

			RegistryKey hardwareConfig = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SYSTEM\\HardwareConfig");

			foreach (string childKeyName in hardwareConfig.GetSubKeyNames())
			{
				RegistryKey childKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey($"SYSTEM\\HardwareConfig\\{childKeyName}");
				childKey.GetValue("");

			}

			return isVirtualMachine;
		}

		private bool IsVirtualWmi()
		{
			bool isVirtualMachine = false;

			ManagementObjectSearcher searcherCpu = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");

			// edge case where no CPU is detected?
			foreach (ManagementObject obj in searcherCpu.Get())
			{
				isVirtualMachine |= ((string) obj["Name"]).ToLower().Contains(" kvm ");  // qemu
			}

			return isVirtualMachine;
		}

		public bool IsVirtualMachine()
		{
			var biosInfo = systemInfo.BiosInfo;
			var isVirtualMachine = false;
			var macAddress = systemInfo.MacAddress;
			var manufacturer = systemInfo.Manufacturer;
			var model = systemInfo.Model;
			var devices = systemInfo.PlugAndPlayDeviceIds;

			isVirtualMachine |= IsVirtualSystemInfo(biosInfo, manufacturer, model);
			isVirtualMachine |= IsVirtualWmi();
			isVirtualMachine |= IsVirtualRegistry();

			// TODO: system version

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
	}
}
