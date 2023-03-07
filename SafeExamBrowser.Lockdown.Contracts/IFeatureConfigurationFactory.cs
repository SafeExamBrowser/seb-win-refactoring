/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;

namespace SafeExamBrowser.Lockdown.Contracts
{
	/// <summary>
	/// The factory for all <see cref="IFeatureConfiguration"/> currently supported.
	/// </summary>
	public interface IFeatureConfigurationFactory
	{
		/// <summary>
		/// Creates all feature configurations.
		/// </summary>
		IList<IFeatureConfiguration> CreateAll(Guid groupId, string sid, string userName);

		/// <summary>
		/// Creates an <see cref="IFeatureConfiguration"/> to control the option to change the password of a user account via the security screen.
		/// </summary>
		IFeatureConfiguration CreateChangePasswordConfiguration(Guid groupId, string sid, string userName);

		/// <summary>
		/// Creates an <see cref="IFeatureConfiguration"/> to control notifications of the Google Chrome browser.
		/// </summary>
		IFeatureConfiguration CreateChromeNotificationConfiguration(Guid groupId, string sid, string userName);

		/// <summary>
		/// Creates an <see cref="IFeatureConfiguration"/> to control the ease of access options on the security screen.
		/// </summary>
		IFeatureConfiguration CreateEaseOfAccessConfiguration(Guid groupId);

		/// <summary>
		/// Creates an <see cref="IFeatureConfiguration"/> to control the find printer option in the print dialog of Windows.
		/// </summary>
		IFeatureConfiguration CreateFindPrinterConfiguration(Guid groupId, string sid, string userName);

		/// <summary>
		/// Creates an <see cref="IFeatureConfiguration"/> to control the option to lock the computer via the security screen.
		/// </summary>
		IFeatureConfiguration CreateLockWorkstationConfiguration(Guid groupId, string sid, string userName);

		/// <summary>
		/// Creates an <see cref="IFeatureConfiguration"/> to control the power options on the security screen.
		/// </summary>
		IFeatureConfiguration CreateMachinePowerOptionsConfiguration(Guid groupId);

		/// <summary>
		/// Creates an <see cref="IFeatureConfiguration"/> to control the network options on the security screen.
		/// </summary>
		IFeatureConfiguration CreateNetworkOptionsConfiguration(Guid groupId);

		/// <summary>
		/// Creates an <see cref="IFeatureConfiguration"/> to control remote desktop connections.
		/// </summary>
		IFeatureConfiguration CreateRemoteConnectionConfiguration(Guid groupId);

		/// <summary>
		/// Creates an <see cref="IFeatureConfiguration"/> to control the option to sign out out via security screen.
		/// </summary>
		IFeatureConfiguration CreateSignoutConfiguration(Guid groupId, string sid, string userName);

		/// <summary>
		/// Creates an <see cref="IFeatureConfiguration"/> to control the option to switch to another user account via the security screen.
		/// </summary>
		IFeatureConfiguration CreateSwitchUserConfiguration(Guid groupId);

		/// <summary>
		/// Creates an <see cref="IFeatureConfiguration"/> to control the task manager of Windows.
		/// </summary>
		IFeatureConfiguration CreateTaskManagerConfiguration(Guid groupId, string sid, string userName);

		/// <summary>
		/// Creates an <see cref="IFeatureConfiguration"/> to control the power options in the start menu.
		/// </summary>
		IFeatureConfiguration CreateUserPowerOptionsConfiguration(Guid groupId, string sid, string userName);

		/// <summary>
		/// Creates an <see cref="IFeatureConfiguration"/> to control the user interface overlay for VMware clients.
		/// </summary>
		IFeatureConfiguration CreateVmwareOverlayConfiguration(Guid groupId, string sid, string userName);

		/// <summary>
		/// Creates an <see cref="IFeatureConfiguration"/> to control Windows Update.
		/// </summary>
		IFeatureConfiguration CreateWindowsUpdateConfiguration(Guid groupId);
	}
}
