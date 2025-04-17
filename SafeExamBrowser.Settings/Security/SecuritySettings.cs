/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;

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
		/// Determines whether the user may reconfigure the application.
		/// </summary>
		public bool AllowReconfiguration { get; set; }

		/// <summary>
		/// Determines whether the user may use the sticky keys feature of the operating system.
		/// </summary>
		public bool AllowStickyKeys { get; set; }

		/// <summary>
		/// Determines whether the user may initiate the termination of SEB. This setting does not affect automated mechanisms like a quit URL.
		/// </summary>
		public bool AllowTermination { get; set; }

		/// <summary>
		/// Determines whether the user interface may be captured by screen recording or remote control software.
		/// </summary>
		public bool AllowWindowCapture { get; set; }

		/// <summary>
		/// Determines whether the user is allowed to use the system clipboard, a custom clipboard or no clipboard at all.
		/// </summary>
		public ClipboardPolicy ClipboardPolicy { get; set; }

		/// <summary>
		/// Determines whether the lock screen is disabled in case of a user session change. This setting overrides the activation based on
		/// <see cref="Service.ServiceSettings.IgnoreService"/> and <see cref="Service.ServiceSettings.DisableUserLock"/> or <see cref="Service.ServiceSettings.DisableUserSwitch"/>.
		/// </summary>
		public bool DisableSessionChangeLockScreen { get; set; }

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
		/// Determines whether the cursor configuration will be verified during session initialization.
		/// </summary>
		public bool VerifyCursorConfiguration { get; set; }

		/// <summary>
		/// Determines whether the session integrity will be verified after session initialization.
		/// </summary>
		public bool VerifySessionIntegrity { get; set; }

		/// <summary>
		/// All restrictions which apply to the SEB version to be used.
		/// </summary>
		public IList<VersionRestriction> VersionRestrictions { get; set; }

		/// <summary>
		/// Determines whether SEB is allowed to run in a virtual machine.
		/// </summary>
		public VirtualMachinePolicy VirtualMachinePolicy { get; set; }

		public SecuritySettings()
		{
			VersionRestrictions = new List<VersionRestriction>();
		}
	}
}
