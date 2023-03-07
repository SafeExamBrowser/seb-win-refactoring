/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Settings
{
	/// <summary>
	/// Defines all possible configuration modes for the application.
	/// </summary>
	public enum ConfigurationMode
	{
		/// <summary>
		/// In this mode, the application settings shall be used to configure the local client settings of a user. When running in this
		/// mode, the user has the possiblity to re-configure the application during runtime.
		/// </summary>
		ConfigureClient,

		/// <summary>
		/// In this mode, the application settings shall only be used to start an exam. When running in this mode, the user cannot re-
		/// configure the application during runtime.
		/// </summary>
		Exam
	}
}
