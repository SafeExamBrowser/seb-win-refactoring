/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace SafeExamBrowser.Configuration.Contracts.Cryptography
{
	/// <summary>
	/// Encrypts and decrypts data with a certificate.
	/// </summary>
	public interface IPublicKeyEncryption
	{
		/// <summary>
		/// Attempts to decrypt the given data. The decrypted data stream and the certificate can only be considered valid if
		/// <see cref="LoadStatus.Success"/> is returned!
		/// </summary>
		LoadStatus Decrypt(Stream data, out Stream decrypted, out X509Certificate2 certificate);

		/// <summary>
		/// Attempts to encrypt the given data. The encrypted data stream can only be considered valid if <see cref="SaveStatus.Success"/>
		/// is returned.
		/// </summary>
		SaveStatus Encrypt(Stream data, X509Certificate2 certificate, out Stream encrypted);
	}
}
