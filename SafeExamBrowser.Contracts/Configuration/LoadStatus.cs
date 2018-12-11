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
	/// Defines all possible results of an attempt to load a configuration resource.
	/// </summary>
	public enum LoadStatus
	{
		/// <summary>
		/// Indicates that the current administrator password is needed to be allowed to configure the local client.
		/// </summary>
		AdminPasswordNeeded = 1,

		/// <summary>
		/// Indicates that a resource contains invalid data.
		/// </summary>
		InvalidData,

		/// <summary>
		/// Indicates that a resource needs to be loaded with the browser.
		/// </summary>
		LoadWithBrowser,

		/// <summary>
		/// Indicates that a resource is not supported.
		/// </summary>
		NotSupported,

		/// <summary>
		/// Indicates that the settings password is needed in order to decrypt the settings.
		/// </summary>
		SettingsPasswordNeeded,

		/// <summary>
		/// The settings were loaded successfully.
		/// </summary>
		Success,

		/// <summary>
		/// The settings were loaded and the local client configuration was performed successfully.
		/// </summary>
		SuccessConfigureClient,

		/// <summary>
		/// An unexpected error occurred while trying to load the settings.
		/// </summary>
		UnexpectedError
	}
}
