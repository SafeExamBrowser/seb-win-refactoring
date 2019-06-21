/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.Lockdown
{
	/// <summary>
	/// The factory for all <see cref="IFeatureConfiguration"/> currently supported.
	/// </summary>
	public interface IFeatureConfigurationFactory
	{
		/// <summary>
		/// Creates an <see cref="IFeatureConfiguration"/> to control notifications of the Google Chrome browser.
		/// </summary>
		IFeatureConfiguration CreateChromeNotificationConfiguration();

		/// <summary>
		/// Creates an <see cref="IFeatureConfiguration"/> to control the ease of access options on the security screen.
		/// </summary>
		IFeatureConfiguration CreateEaseOfAccessConfiguration();

		/// <summary>
		/// Creates an <see cref="IFeatureConfiguration"/> to control the network options on the security screen.
		/// </summary>
		IFeatureConfiguration CreateNetworkOptionsConfiguration();

		/// <summary>
		/// Creates an <see cref="IFeatureConfiguration"/> to control the option to change the password of a user account via the security screen.
		/// </summary>
		IFeatureConfiguration CreatePasswordChangeConfiguration();

		/// <summary>
		/// Creates an <see cref="IFeatureConfiguration"/> to control the power options on the security screen.
		/// </summary>
		IFeatureConfiguration CreatePowerOptionsConfiguration();

		/// <summary>
		/// Creates an <see cref="IFeatureConfiguration"/> to control remote desktop connections.
		/// </summary>
		IFeatureConfiguration CreateRemoteConnectionConfiguration();

		/// <summary>
		/// Creates an <see cref="IFeatureConfiguration"/> to control the option to sign out out via security screen.
		/// </summary>
		IFeatureConfiguration CreateSignoutConfiguration();

		/// <summary>
		/// Creates an <see cref="IFeatureConfiguration"/> to control the task manager of Windows.
		/// </summary>
		IFeatureConfiguration CreateTaskManagerConfiguration();

		/// <summary>
		/// Creates an <see cref="IFeatureConfiguration"/> to control the option to lock the computer via the security screen.
		/// </summary>
		IFeatureConfiguration CreateUserLockConfiguration();

		/// <summary>
		/// Creates an <see cref="IFeatureConfiguration"/> to control the option to switch to another user account via the security screen.
		/// </summary>
		IFeatureConfiguration CreateUserSwitchConfiguration();

		/// <summary>
		/// Creates an <see cref="IFeatureConfiguration"/> to control the user interface overlay for VMware clients.
		/// </summary>
		IFeatureConfiguration CreateVmwareOverlayConfiguration();

		/// <summary>
		/// Creates an <see cref="IFeatureConfiguration"/> to control Windows Update.
		/// </summary>
		IFeatureConfiguration CreateWindowsUpdateConfiguration();
	}
}
