/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Communication.Contracts.Data
{
	/// <summary>
	/// Defines all possible reasons for a <see cref="PasswordRequestMessage"/>.
	/// </summary>
	public enum PasswordRequestPurpose
	{
		/// <summary>
		/// The password is to be used as administrator password for the local client configuration.
		/// </summary>
		LocalAdministrator,

		/// <summary>
		/// The password is to be used as settings password for the local client configuration.
		/// </summary>
		LocalSettings,

		/// <summary>
		/// The password is to be used as settings password for an application configuration.
		/// </summary>
		Settings
	}
}
