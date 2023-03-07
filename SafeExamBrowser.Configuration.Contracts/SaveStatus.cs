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
	/// Defines all possible results of an attempt to save a configuration resource.
	/// </summary>
	public enum SaveStatus
	{
		/// <summary>
		/// The configuration data is invalid or contains invalid elements.
		/// </summary>
		InvalidData,

		/// <summary>
		/// The configuration format or resource type is not supported.
		/// </summary>
		NotSupported,

		/// <summary>
		/// The configuration was saved successfully.
		/// </summary>
		Success,

		/// <summary>
		/// An unexpected error occurred while trying to save the configuration.
		/// </summary>
		UnexpectedError
	}
}
