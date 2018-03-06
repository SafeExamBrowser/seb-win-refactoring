/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.Configuration
{
	/// <summary>
	/// Provides access to information about the operating system.
	/// </summary>
	public interface ISystemInfo
	{
		/// <summary>
		/// Reveals whether the computer system contains a battery.
		/// </summary>
		bool HasBattery { get; }

		/// <summary>
		/// Reveals the version of the currently running operating system.
		/// </summary>
		OperatingSystem OperatingSystem { get; }

		/// <summary>
		/// Provides detailed version information about the currently running operating system.
		/// </summary>
		string OperatingSystemInfo { get; }
	}
}
