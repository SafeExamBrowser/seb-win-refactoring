/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Configuration.Cryptography
{
	public class PublicKeyEncryption : IPublicKeyEncryption
	{
		protected const int PUBLIC_KEY_HASH_SIZE = 20;

		protected ICertificateStore store;
		protected ILogger logger;

		public PublicKeyEncryption(ICertificateStore store, ILogger logger)
		{
			this.logger = logger;
			this.store = store;
		}

		public virtual LoadStatus Decrypt(Stream data, out Stream decryptedData, out X509Certificate2 certificate)
		{
			var publicKeyHash = ParsePublicKeyHash(data);
			var found = store.TryGetCertificateWith(publicKeyHash, out certificate);

			decryptedData = default(Stream);

			if (!found)
			{
				return FailForMissingCertificate();
			}

			decryptedData = Decrypt(data, PUBLIC_KEY_HASH_SIZE, certificate);

			return LoadStatus.Success;
		}

		public virtual SaveStatus Encrypt(Stream data, X509Certificate2 certificate, out Stream encryptedData)
		{
			var publicKeyHash = GeneratePublicKeyHash(certificate);

			encryptedData = Encrypt(data, certificate);
			encryptedData = WriteEncryptionParameters(encryptedData, publicKeyHash);

			return SaveStatus.Success;
		}

		protected LoadStatus FailForMissingCertificate()
		{
			logger.Error($"Could not find certificate which matches the given public key hash!");

			return LoadStatus.InvalidData;
		}

		protected byte[] GeneratePublicKeyHash(X509Certificate2 certificate)
		{
			var publicKey = certificate.PublicKey.EncodedKeyValue.RawData;

			using (var sha = new SHA1CryptoServiceProvider())
			{
				return sha.ComputeHash(publicKey);
			}
		}

		protected byte[] ParsePublicKeyHash(Stream data)
		{
			var keyHash = new byte[PUBLIC_KEY_HASH_SIZE];

			logger.Debug("Parsing public key hash...");

			data.Seek(0, SeekOrigin.Begin);
			data.Read(keyHash, 0, keyHash.Length);

			return keyHash;
		}

		protected MemoryStream Decrypt(Stream data, long offset, X509Certificate2 certificate)
		{
			var algorithm = certificate.PrivateKey as RSACryptoServiceProvider;
			var blockSize = algorithm.KeySize / 8;
			var blockCount = (data.Length - offset) / blockSize;
			var decrypted = new MemoryStream();
			var decryptedBuffer = new byte[blockSize];
			var encryptedBuffer = new byte[blockSize];
			var remainingBytes = data.Length - offset - (blockSize * blockCount);

			data.Seek(offset, SeekOrigin.Begin);
			logger.Debug("Decrypting data...");

			using (algorithm)
			{
				for (var block = 0; block < blockCount; block++)
				{
					data.Read(encryptedBuffer, 0, encryptedBuffer.Length);
					decryptedBuffer = algorithm.Decrypt(encryptedBuffer, false);
					decrypted.Write(decryptedBuffer, 0, decryptedBuffer.Length);
				}

				if (remainingBytes > 0)
				{
					encryptedBuffer = new byte[remainingBytes];
					data.Read(encryptedBuffer, 0, encryptedBuffer.Length);
					decryptedBuffer = algorithm.Decrypt(encryptedBuffer, false);
					decrypted.Write(decryptedBuffer, 0, decryptedBuffer.Length);
				}
			}

			return decrypted;
		}

		protected Stream Encrypt(Stream data, X509Certificate2 certificate)
		{
			var algorithm = certificate.PublicKey.Key as RSACryptoServiceProvider;
			var blockSize = (algorithm.KeySize / 8) - 32;
			var blockCount = data.Length / blockSize;
			var decryptedBuffer = new byte[blockSize];
			var encrypted = new MemoryStream();
			var encryptedBuffer = new byte[blockSize];
			var remainingBytes = data.Length - (blockCount * blockSize);

			data.Seek(0, SeekOrigin.Begin);
			logger.Debug("Encrypting data...");

			using (algorithm)
			{
				for (var block = 0; block < blockCount; block++)
				{
					data.Read(decryptedBuffer, 0, decryptedBuffer.Length);
					encryptedBuffer = algorithm.Encrypt(decryptedBuffer, false);
					encrypted.Write(encryptedBuffer, 0, encryptedBuffer.Length);
				}

				if (remainingBytes > 0)
				{
					decryptedBuffer = new byte[remainingBytes];
					data.Read(decryptedBuffer, 0, decryptedBuffer.Length);
					encryptedBuffer = algorithm.Encrypt(decryptedBuffer, false);
					encrypted.Write(encryptedBuffer, 0, encryptedBuffer.Length);
				}
			}

			return encrypted;
		}

		private Stream WriteEncryptionParameters(Stream encryptedData, byte[] keyHash)
		{
			var data = new MemoryStream();

			logger.Debug("Writing encryption parameters...");
			data.Write(keyHash, 0, keyHash.Length);
			encryptedData.Seek(0, SeekOrigin.Begin);
			encryptedData.CopyTo(data);

			return data;
		}
	}
}
