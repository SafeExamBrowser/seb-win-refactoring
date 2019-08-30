/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows.Forms;
using SafeExamBrowser.SystemComponents.Contracts;
using BatteryChargeStatus = System.Windows.Forms.BatteryChargeStatus;

namespace SafeExamBrowser.SystemComponents
{
	public class SystemInfo : ISystemInfo
	{
		public bool HasBattery { get; private set; }
		public OperatingSystem OperatingSystem { get; private set; }

		public string OperatingSystemInfo
		{
			get { return $"{OperatingSystemName()}, {System.Environment.OSVersion.VersionString} ({Architecture()})"; }
		}

		public SystemInfo()
		{
			InitializeBattery();
			InitializeOperatingSystem();
		}

		private void InitializeBattery()
		{
			var status = SystemInformation.PowerStatus.BatteryChargeStatus;

			HasBattery = !status.HasFlag(BatteryChargeStatus.NoSystemBattery) && !status.HasFlag(BatteryChargeStatus.Unknown);
		}

		private void InitializeOperatingSystem()
		{
			// IMPORTANT:
			// In order to be able to retrieve the correct operating system version via System.Environment.OSVersion, the executing
			// assembly needs to define an application manifest where the supported Windows versions are specified!
			var major = System.Environment.OSVersion.Version.Major;
			var minor = System.Environment.OSVersion.Version.Minor;

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
				OperatingSystem = OperatingSystem.Windows10;
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
				default:
					return "Unknown Windows Version";
			}
		}

		private string Architecture()
		{
			return System.Environment.Is64BitOperatingSystem ? "x64" : "x86";
		}
	}
}
