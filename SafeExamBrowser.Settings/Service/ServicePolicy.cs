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
	/// Defines all possible service policies which the application supports.
	/// </summary>
	public enum ServicePolicy
	{
		/// <summary>
		/// The service component must be running. If it is not running, the user won't be able to start the application.
		/// </summary>
		Mandatory,

		/// <summary>
		/// The service component is optional. If it is not running, all service-related actions are simply skipped.
		/// </summary>
		Optional,

		/// <summary>
		/// The service component should be running. If it is not running, the user will be warned that all service-related actions are skipped.
		/// </summary>
		Warn
	}
}
