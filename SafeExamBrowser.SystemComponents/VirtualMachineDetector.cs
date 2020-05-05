/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;

namespace SafeExamBrowser.SystemComponents
{
	public class VirtualMachineDetector : IVirtualMachineDetector
	{
		private ILogger logger;
		private ISystemInfo systemInfo;
		private static readonly string[] pciVendorBlacklist = { "vbox", "80ee", "qemu", "1af4", "1b36" }; //Virtualbox: VBOX, 80EE   RedHat: QUEMU, 1AF4, 1B36
		public VirtualMachineDetector(ILogger logger, ISystemInfo systemInfo)
		{
			this.logger = logger;
			this.systemInfo = systemInfo;
		}

		public bool IsVirtualMachine()
		{
			var isVirtualMachine = false;
			var manufacturer = systemInfo.Manufacturer.ToLower();
			var model = systemInfo.Model.ToLower();

			isVirtualMachine |= manufacturer.Contains("microsoft corporation") && !model.Contains("surface");
			isVirtualMachine |= manufacturer.Contains("vmware");
			isVirtualMachine |= manufacturer.Contains("parallels software");
			isVirtualMachine |= model.Contains("virtualbox");
			isVirtualMachine |= manufacturer.Contains("qemu");
			
			var macAddr = (from nic in NetworkInterface.GetAllNetworkInterfaces() where nic.OperationalStatus == OperationalStatus.Up select nic.GetPhysicalAddress().ToString()).FirstOrDefault();
			isVirtualMachine |= ((byte.Parse(macAddr[1].ToString(), NumberStyles.HexNumber) & 2) == 2 || macAddr.StartsWith("080027"));

			using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT DeviceID FROM Win32_PnPEntity"))

				foreach (ManagementObject queryObj in searcher.Get())
				{
				isVirtualMachine |= pciVendorBlacklist.Any(System.Convert.ToString(queryObj.Properties.Cast<PropertyData>().First().Value).ToLower().Contains);
					
				}

			logger.Debug($"Computer '{systemInfo.Name}' appears to {(isVirtualMachine ? "" : "not ")}be a virtual machine.");

			return isVirtualMachine;
		}
	}
}
