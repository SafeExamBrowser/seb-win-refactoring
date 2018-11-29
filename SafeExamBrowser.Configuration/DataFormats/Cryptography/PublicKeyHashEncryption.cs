/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Configuration.DataFormats.Cryptography
{
	internal class PublicKeyHashEncryption
	{
		protected const int PUBLIC_KEY_HASH_SIZE = 20;

		protected ILogger logger;

		internal PublicKeyHashEncryption(ILogger logger)
		{
			this.logger = logger;
		}

		internal virtual LoadStatus Decrypt(Stream data, out Stream decrypted)
		{
			var keyHash = ParsePublicKeyHash(data);
			var found = TryGetCertificateWith(keyHash, out X509Certificate2 certificate);

			decrypted = default(Stream);

			if (!found)
			{
				return FailForMissingCertificate();
			}

			decrypted = Decrypt(data, PUBLIC_KEY_HASH_SIZE, certificate);

			return LoadStatus.Success;
		}

		protected byte[] ParsePublicKeyHash(Stream data)
		{
			var keyHash = new byte[PUBLIC_KEY_HASH_SIZE];

			logger.Debug("Parsing public key hash...");

			data.Seek(0, SeekOrigin.Begin);
			data.Read(keyHash, 0, keyHash.Length);

			return keyHash;
		}

		protected bool TryGetCertificateWith(byte[] keyHash, out X509Certificate2 certificate)
		{
			var storesToSearch = new[]
			{
				new X509Store(StoreLocation.CurrentUser),
				new X509Store(StoreLocation.LocalMachine),
				new X509Store(StoreName.TrustedPeople)
			};

			certificate = default(X509Certificate2);
			logger.Debug("Searching certificate for decryption...");

			using (var algorithm = new SHA1CryptoServiceProvider())
			{
				foreach (var store in storesToSearch)
				{
					store.Open(OpenFlags.ReadOnly);

					foreach (var current in store.Certificates)
					{
						var publicKey = current.PublicKey.EncodedKeyValue.RawData;
						var publicKeyHash = algorithm.ComputeHash(publicKey);

						if (publicKeyHash.SequenceEqual(keyHash))
						{
							certificate = current;
							store.Close();

							return true;
						}
					}

					store.Close();
				}
			}

			return false;
		}

		protected LoadStatus FailForMissingCertificate()
		{
			logger.Error($"Could not find certificate which matches the given public key hash!");

			return LoadStatus.InvalidData;
		}

		protected MemoryStream Decrypt(Stream data, long offset, X509Certificate2 certificate)
		{
			var algorithm = certificate.PrivateKey as RSACryptoServiceProvider;
			var blockSize = algorithm.KeySize / 8;
			var blockCount = (data.Length - offset) / blockSize;
			var remainingBytes = data.Length - offset - (blockSize * blockCount);
			var encrypted = new byte[blockSize];
			var decrypted = default(byte[]);
			var decryptedData = new MemoryStream();

			data.Seek(offset, SeekOrigin.Begin);
			logger.Debug("Decrypting data...");

			for (int i = 0; i < blockCount; i++)
			{
				data.Read(encrypted, 0, encrypted.Length);
				decrypted = algorithm.Decrypt(encrypted, false);
				decryptedData.Write(decrypted, 0, decrypted.Length);
			}

			if (remainingBytes > 0)
			{
				encrypted = new byte[remainingBytes];
				data.Read(encrypted, 0, encrypted.Length);
				decrypted = algorithm.Decrypt(encrypted, false);
				decryptedData.Write(decrypted, 0, decrypted.Length);
			}

			return decryptedData;
		}
	}
}
