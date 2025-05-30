﻿/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Applications.Contracts
{
	/// <summary>
	/// Defines all possible results of an attempt to create an application.
	/// </summary>
	public enum FactoryResult
	{
		/// <summary>
		/// An error occurred while trying to create the application.
		/// </summary>
		Error,

		/// <summary>
		/// The application has been found but is invalid (e.g. because it is not the correct version or has been manipulated).
		/// </summary>
		Invalid,

		/// <summary>
		/// The application could not be found on the system.
		/// </summary>
		NotFound,

		/// <summary>
		/// The application has been created successfully.
		/// </summary>
		Success
	}
}
