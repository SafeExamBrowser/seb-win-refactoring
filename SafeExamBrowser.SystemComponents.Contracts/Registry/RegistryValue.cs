/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.SystemComponents.Contracts.Registry
{
	/// <summary>
	/// Defines registry values used in conjunction with <see cref="IRegistry"/>. Use the pattern "LogicalGroup_Key" resp. "LogicalGroup_Name" to
	/// allow for a better overview over all values and their usage (where applicable).
	/// </summary>
	public static class RegistryValue
	{
		/// <summary>
		/// All registry values located in the machine hive.
		/// </summary>
		public static class MachineHive
		{
			public const string AppPaths_Key = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths";
			public const string EaseOfAccess_Key = @"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\Utilman.exe";
			public const string EaseOfAccess_Name = "Debugger";
			public const string HardwareConfig_Key = @"HKEY_LOCAL_MACHINE\SYSTEM\HardwareConfig";
		}

		/// <summary>
		/// All registry values located in the user hive.
		/// </summary>
		public static class UserHive
		{
			public const string Cursors_Key = @"HKEY_CURRENT_USER\Control Panel\Cursors";
			public const string Cursors_AppStarting_Name = "AppStarting";
			public const string Cursors_Arrow_Name = "Arrow";
			public const string Cursors_Crosshair_Name = "Crosshair";
			public const string Cursors_Hand_Name = "Hand";
			public const string Cursors_Help_Name = "Help";
			public const string Cursors_IBeam_Name = "IBeam";
			public const string Cursors_No_Name = "No";
			public const string Cursors_NWPen_Name = "NWPen";
			public const string Cursors_Person_Name = "Person";
			public const string Cursors_Pin_Name = "Pin";
			public const string Cursors_SizeAll_Name = "SizeAll";
			public const string Cursors_SizeNESW_Name = "SizeNESW";
			public const string Cursors_SizeNS_Name = "SizeNS";
			public const string Cursors_SizeNWSE_Name = "SizeNWSE";
			public const string Cursors_SizeWE_Name = "SizeWE";
			public const string Cursors_UpArrow_Name = "UpArrow";
			public const string Cursors_Wait_Name = "Wait";
			public const string DeviceCache_Key = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\TaskFlow\DeviceCache";
			public const string NoDrives_Key = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer";
			public const string NoDrives_Name = "NoDrives";
		}
	}
}
