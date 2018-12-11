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
	/// Holds all password data necessary to load an application configuration.
	/// </summary>
	public class PasswordInfo
	{
		/// <summary>
		/// The current administrator password in plain text.
		/// </summary>
		public string AdminPassword { get; set; }

		/// <summary>
		/// The hash code of the current administrator password.
		/// </summary>
		public string AdminPasswordHash { get; set; }

		/// <summary>
		/// The settings password of the configuration in plain text.
		/// </summary>
		public string SettingsPassword { get; set; }
	}
}
