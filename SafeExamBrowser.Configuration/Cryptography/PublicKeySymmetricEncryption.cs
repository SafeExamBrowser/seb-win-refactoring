/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Configuration.Cryptography
{
	public class PublicKeySymmetricEncryption : PublicKeyEncryption
	{
		private const int ENCRYPTION_KEY_LENGTH = 32;
		private const int KEY_LENGTH_SIZE = 4;

		private PasswordEncryption passwordEncryption;

		public PublicKeySymmetricEncryption(ICertificateStore store, ILogger logger, PasswordEncryption passwordEncryption) : base(store, logger)
		{
			this.passwordEncryption = passwordEncryption;
		}

		public override LoadStatus Decrypt(Stream data, out Stream decryptedData, out X509Certificate2 certificate)
		{
			var publicKeyHash = ParsePublicKeyHash(data);
			var found = store.TryGetCertificateWith(publicKeyHash, out certificate);

			decryptedData = default(Stream);

			if (!found)
			{
				return FailForMissingCertificate();
			}

			var symmetricKey = ParseSymmetricKey(data, certificate);
			var stream = new SubStream(data, data.Position, data.Length - data.Position);
			var status = passwordEncryption.Decrypt(stream, symmetricKey, out decryptedData);

			return status;
		}

		public override SaveStatus Encrypt(Stream data, X509Certificate2 certificate, out Stream encryptedData)
		{
			var publicKeyHash = GeneratePublicKeyHash(certificate);
			var symmetricKey = GenerateSymmetricKey();
			var symmetricKeyString = Convert.ToBase64String(symmetricKey);
			var status = passwordEncryption.Encrypt(data, symmetricKeyString, out encryptedData);

			if (status != SaveStatus.Success)
			{
				return FailForUnsuccessfulPasswordEncryption(status);
			}

			encryptedData = WriteEncryptionParameters(encryptedData, certificate, publicKeyHash, symmetricKey);

			return SaveStatus.Success;
		}

		private SaveStatus FailForUnsuccessfulPasswordEncryption(SaveStatus status)
		{
			logger.Error($"Password encryption has failed with status '{status}'!");

			return SaveStatus.UnexpectedError;
		}

		private byte[] GenerateSymmetricKey()
		{
			var key = new byte[ENCRYPTION_KEY_LENGTH];

			using (var generator = RandomNumberGenerator.Create())
			{
				generator.GetBytes(key);
			}

			return key;
		}

		private string ParseSymmetricKey(Stream data, X509Certificate2 certificate)
		{
			var keyLengthData = new byte[KEY_LENGTH_SIZE];

			logger.Debug("Parsing symmetric key...");

			data.Seek(PUBLIC_KEY_HASH_SIZE, SeekOrigin.Begin);
			data.Read(keyLengthData, 0, keyLengthData.Length);

			var encryptedKeyLength = BitConverter.ToInt32(keyLengthData, 0);
			var encryptedKey = new byte[encryptedKeyLength];

			data.Read(encryptedKey, 0, encryptedKey.Length);

			var stream = new SubStream(data, PUBLIC_KEY_HASH_SIZE + KEY_LENGTH_SIZE, encryptedKeyLength);
			var decryptedKey = Decrypt(stream, 0, certificate);
			var symmetricKey = Convert.ToBase64String(decryptedKey.ToArray());

			return symmetricKey;
		}

		private Stream WriteEncryptionParameters(Stream encryptedData, X509Certificate2 certificate, byte[] publicKeyHash, byte[] symmetricKey)
		{
			var data = new MemoryStream();
			var symmetricKeyData = new MemoryStream(symmetricKey);
			var encryptedKey = Encrypt(symmetricKeyData, certificate);
			// IMPORTANT: The key length must be exactly 4 Bytes, thus the cast to integer!
			var encryptedKeyLength = BitConverter.GetBytes((int) encryptedKey.Length);

			logger.Debug("Writing encryption parameters...");

			data.Write(publicKeyHash, 0, publicKeyHash.Length);
			data.Write(encryptedKeyLength, 0, encryptedKeyLength.Length);

			encryptedKey.Seek(0, SeekOrigin.Begin);
			encryptedKey.CopyTo(data);

			encryptedData.Seek(0, SeekOrigin.Begin);
			encryptedData.CopyTo(data);

			return data;
		}
	}
}
