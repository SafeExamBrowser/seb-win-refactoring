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


namespace SafeExamBrowser.SystemComponents
{
	public class VirtualMachineDetector : IVirtualMachineDetector
	{
		private static readonly string[] PCI_VENDOR_BLACKLIST = { "vbox", "80ee", "qemu", "1af4", "1b36" }; //Virtualbox: VBOX, 80EE   RedHat: QUEMU, 1AF4, 1B36

		private ILogger logger;
		private ISystemInfo systemInfo;
		
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
			var macAddress = systemInfo.MacAddress;

			isVirtualMachine |= manufacturer.Contains("microsoft corporation") && !model.Contains("surface");
			isVirtualMachine |= manufacturer.Contains("vmware");
			isVirtualMachine |= manufacturer.Contains("parallels software");
			isVirtualMachine |= model.Contains("virtualbox");
			isVirtualMachine |= manufacturer.Contains("qemu");
			
			isVirtualMachine |= ((byte.Parse(macAddress[1].ToString(), NumberStyles.HexNumber) & 2) == 2 || macAddress.StartsWith("080027"));
			/*
			using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT DeviceID FROM Win32_PnPEntity"))

				foreach (ManagementObject queryObj in searcher.Get())
				{
				isVirtualMachine |= pciVendorBlacklist.Any(System.Convert.ToString(queryObj.Properties.Cast<PropertyData>().First().Value).ToLower().Contains);
					
				}
				*/
			logger.Debug($"Computer '{systemInfo.Name}' appears to {(isVirtualMachine ? "" : "not ")}be a virtual machine.");

			return isVirtualMachine;
		}
	}
}
