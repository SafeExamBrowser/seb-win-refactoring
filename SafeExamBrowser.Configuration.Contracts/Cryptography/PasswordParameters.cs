/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Configuration.Contracts.Cryptography
{
	/// <summary>
	/// Holds all parameters for data encryption by password.
	/// </summary>
	public class PasswordParameters : EncryptionParameters
	{
		/// <summary>
		/// The password in plain text.
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// Indicates whether the password is a hash code.
		/// </summary>
		public bool IsHash { get; set; }
	}
}
