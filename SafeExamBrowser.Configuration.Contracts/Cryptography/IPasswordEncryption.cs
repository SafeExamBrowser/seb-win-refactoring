/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.IO;

namespace SafeExamBrowser.Configuration.Contracts.Cryptography
{
	/// <summary>
	/// Encrypts and decrypts data with a password.
	/// </summary>
	public interface IPasswordEncryption
	{
		/// <summary>
		/// Attempts to decrypt the given data. The decrypted data stream can only be considered valid if <see cref="LoadStatus.Success"/>
		/// is returned!
		/// </summary>
		LoadStatus Decrypt(Stream data, string password, out Stream decrypted);

		/// <summary>
		/// Attempts to encrypt the given data. The encrypted data stream can only be considered valid if <see cref="SaveStatus.Success"/>
		/// is returned.
		/// </summary>
		SaveStatus Encrypt(Stream data, string password, out Stream encrypted);
	}
}
