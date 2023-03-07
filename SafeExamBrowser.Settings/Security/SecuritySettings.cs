/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Settings.Security
{
	/// <summary>
	/// Defines all settings related to security.
	/// </summary>
	[Serializable]
	public class SecuritySettings
	{
		/// <summary>
		/// The hash code of the administrator password for the settings.
		/// </summary>
		public string AdminPasswordHash { get; set; }

		/// <summary>
		/// Determines whether any log information will be accessible via the user interface.
		/// </summary>
		public bool AllowApplicationLogAccess { get; set; }

		/// <summary>
		/// Determines whether the user may initiate the termination of SEB. This setting does not affect automated mechanisms like a quit URL.
		/// </summary>
		public bool AllowTermination { get; set; }

		/// <summary>
		/// Determines whether the user may reconfigure the application.
		/// </summary>
		public bool AllowReconfiguration { get; set; }

		/// <summary>
		/// The kiosk mode which determines how the computer is locked down.
		/// </summary>
		public KioskMode KioskMode { get; set; }

		/// <summary>
		/// The hash code of the quit password.
		/// </summary>
		public string QuitPasswordHash { get; set; }

		/// <summary>
		/// An URL to optionally restrict with which resource SEB may be reconfigured. Allows the usage of a wildcard character (<c>*</c>).
		/// </summary>
		public string ReconfigurationUrl { get; set; }

		/// <summary>
		/// Determines whether SEB is allowed to run in a virtual machine.
		/// </summary>
		public VirtualMachinePolicy VirtualMachinePolicy { get; set; }
	}
}
