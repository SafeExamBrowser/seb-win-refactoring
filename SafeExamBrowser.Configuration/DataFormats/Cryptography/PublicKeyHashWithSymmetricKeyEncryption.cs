/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Configuration.DataFormats.Cryptography
{
	internal class PublicKeyHashWithSymmetricKeyEncryption : PublicKeyHashEncryption
	{
		private const int KEY_LENGTH_SIZE = 4;

		private PasswordEncryption passwordEncryption;

		internal PublicKeyHashWithSymmetricKeyEncryption(ILogger logger, PasswordEncryption passwordEncryption) : base(logger)
		{
			this.passwordEncryption = passwordEncryption;
		}

		internal override LoadStatus Decrypt(Stream data, out Stream decrypted)
		{
			var keyHash = ParsePublicKeyHash(data);
			var found = TryGetCertificateWith(keyHash, out X509Certificate2 certificate);

			decrypted = default(Stream);

			if (!found)
			{
				return FailForMissingCertificate();
			}

			var symmetricKey = ParseSymmetricKey(data, certificate);
			var stream = new SubStream(data, data.Position, data.Length - data.Position);
			var status = passwordEncryption.Decrypt(stream, symmetricKey, out decrypted);

			return status;
		}

		private string ParseSymmetricKey(Stream data, X509Certificate2 certificate)
		{
			var keyLengthData = new byte[KEY_LENGTH_SIZE];

			logger.Debug("Parsing symmetric key...");

			data.Seek(PUBLIC_KEY_HASH_SIZE, SeekOrigin.Begin);
			data.Read(keyLengthData, 0, keyLengthData.Length);

			var keyLength = BitConverter.ToInt32(keyLengthData, 0);
			var encryptedKey = new byte[keyLength];

			data.Read(encryptedKey, 0, encryptedKey.Length);

			var stream = new SubStream(data, PUBLIC_KEY_HASH_SIZE + KEY_LENGTH_SIZE, keyLength);
			var decryptedKey = Decrypt(stream, 0, certificate);
			var symmetricKey = Convert.ToBase64String(decryptedKey.ToArray());

			return symmetricKey;
		}
	}
}
