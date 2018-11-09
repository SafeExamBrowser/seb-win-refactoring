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
		/// Indicates that an admin password is needed in order to load the settings.
		/// </summary>
		AdminPasswordNeeded = 1,

		/// <summary>
		/// Indicates that a resource does not comply with the declared data format.
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
		/// Indicates that a settings password is needed in order to load the settings.
		/// </summary>
		SettingsPasswordNeeded,

		/// <summary>
		/// The settings were loaded successfully.
		/// </summary>
		Success,

		/// <summary>
		/// An unexpected error occurred while trying to load the settings.
		/// </summary>
		UnexpectedError
	}
}
