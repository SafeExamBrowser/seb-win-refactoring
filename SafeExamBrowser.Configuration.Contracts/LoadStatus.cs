/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Configuration.Contracts
{
	/// <summary>
	/// Defines all possible results of an attempt to load a configuration resource.
	/// </summary>
	public enum LoadStatus
	{
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
		/// Indicates that a password is needed in order to decrypt the configuration.
		/// </summary>
		PasswordNeeded,

		/// <summary>
		/// The configuration was loaded successfully.
		/// </summary>
		Success,

		/// <summary>
		/// An unexpected error occurred while trying to load the configuration.
		/// </summary>
		UnexpectedError
	}
}
