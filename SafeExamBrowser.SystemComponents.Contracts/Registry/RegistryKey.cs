/*
 * Copyright (c) 2022 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.SystemComponents.Contracts.Registry
{
	/// <summary>
	/// Defines registry keys and names used in conjunction with <see cref="IRegistry"/>. Use the pattern "LogicalGroup_Key" resp. "LogicalGroup_Name"
	/// to allow for a better overview over all keys, names and their usage (where applicable).
	/// </summary>
	public static class RegistryKey
	{
		/// <summary>
		/// All registry keys and names located in the machine hive.
		/// </summary>
		public static class MachineHive
		{
			public const string EaseOfAccess_Key = @"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\Utilman.exe";
			public const string EaseOfAccess_Name = "Debugger";
		}
	}
}
