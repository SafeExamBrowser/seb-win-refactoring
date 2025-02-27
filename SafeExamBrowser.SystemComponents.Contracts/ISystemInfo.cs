/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.IO;

namespace SafeExamBrowser.SystemComponents.Contracts
{
	/// <summary>
	/// Provides access to information about the computer system.
	/// </summary>
	public interface ISystemInfo
	{
		/// <summary>
		/// The manufacturer and name of the BIOS.
		/// </summary>
		string BiosInfo { get; }

		/// <summary>
		/// The name of the CPU.
		/// </summary>
		string CpuName { get; }

		/// <summary>
		/// Reveals whether the computer system contains a battery.
		/// </summary>
		bool HasBattery { get; }

		/// <summary>
		/// The MAC address of the network adapter.
		/// </summary>
		string MacAddress { get; }

		/// <summary>
		/// The manufacturer name of the computer system.
		/// </summary>
		string Manufacturer { get; }

		/// <summary>
		/// The model name of the computer system.
		/// </summary>
		string Model { get; }

		/// <summary>
		/// The machine name of the computer system.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Reveals the version of the currently running operating system.
		/// </summary>
		OperatingSystem OperatingSystem { get; }

		/// <summary>
		/// Provides detailed version information about the currently running operating system.
		/// </summary>
		string OperatingSystemInfo { get; }

		/// <summary>
		/// Provides the device ID information of the user's Plug and Play devices.
		/// </summary>
		string[] PlugAndPlayDeviceIds { get; }

		/// <summary>
		/// Retrieves all logical drives of the computer system.
		/// </summary>
		IEnumerable<DriveInfo> GetDrives();
	}
}
