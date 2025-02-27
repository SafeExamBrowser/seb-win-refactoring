/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.UserInterface.Contracts.Windows.Data
{
	/// <summary>
	/// Defines the purpose of a <see cref="ICredentialsDialog"/>.
	/// </summary>
	public enum CredentialsDialogPurpose
	{
		/// <summary>
		/// Credentials for generic purposes.
		/// </summary>
		Generic,

		/// <summary>
		/// Credentials for wireless network authentication.
		/// </summary>
		WirelessNetwork
	}
}
