/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Settings.Service
{
	/// <summary>
	/// Defines all settings for the service application component.
	/// </summary>
	public class ServiceSettings
	{
		/// <summary>
		/// Determines whether desktop notifications of Google Chrome should be deactivated.
		/// </summary>
		public bool DisableChromeNotifications { get; set; }

		/// <summary>
		/// Determines whether the user can access the ease of access options on the security screen.
		/// </summary>
		public bool DisableEaseOfAccessOptions { get; set; }

		/// <summary>
		/// Determines whether the user can access the find printer option in the print dialog of Windows.
		/// </summary>
		public bool DisableFindPrinter { get; set; }

		/// <summary>
		/// Determines whether the user can access the network options on the security screen.
		/// </summary>
		public bool DisableNetworkOptions { get; set; }

		/// <summary>
		/// Determines whether the user can change the password for a user account via the security screen.
		/// </summary>
		public bool DisablePasswordChange { get; set; }

		/// <summary>
		/// Determines whether the user can access the power options on the security screen.
		/// </summary>
		public bool DisablePowerOptions { get; set; }

		/// <summary>
		/// Determines whether remote desktop connections should be deactivated.
		/// </summary>
		public bool DisableRemoteConnections { get; set; }

		/// <summary>
		/// Determines whether the user can sign out of their account via the security screen.
		/// </summary>
		public bool DisableSignout { get; set; }

		/// <summary>
		/// Determines whether the user can access the task manager of Windows.
		/// </summary>
		public bool DisableTaskManager { get; set; }

		/// <summary>
		/// Determines whether the user can lock the computer via the security screen.
		/// </summary>
		public bool DisableUserLock { get; set; }

		/// <summary>
		/// Determines whether the user can switch to another user account via the security screen.
		/// </summary>
		public bool DisableUserSwitch { get; set; }

		/// <summary>
		/// Determines whether the user interface overlay for VMware clients should be deactivated.
		/// </summary>
		public bool DisableVmwareOverlay { get; set; }

		/// <summary>
		/// Determines whether Windows Update should be deactivated.
		/// </summary>
		public bool DisableWindowsUpdate { get; set; }

		/// <summary>
		/// Determines whether the service will be completely ignored, i.e. no actions will be performed by the service component.
		/// </summary>
		public bool IgnoreService { get; set; }

		/// <summary>
		/// The active policy for the service component. Has no effect if <see cref="IgnoreService"/> is set to <c>true</c>.
		/// </summary>
		public ServicePolicy Policy { get; set; }

		/// <summary>
		/// Determines whether the VMware configuration will be set by the service.
		/// </summary>
		public bool SetVmwareConfiguration { get; set; }
	}
}
