/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using SafeExamBrowser.Contracts.Configuration.Cryptography;

namespace SafeExamBrowser.Configuration.Cryptography
{
	internal class CertificateStore : ICertificateStore
	{
		private readonly X509Store[] stores = new[]
		{
			new X509Store(StoreLocation.CurrentUser),
			new X509Store(StoreLocation.LocalMachine),
			new X509Store(StoreName.TrustedPeople)
		};

		public bool TryGetCertificateWith(byte[] keyHash, out X509Certificate2 certificate)
		{
			certificate = default(X509Certificate2);

			using (var algorithm = new SHA1CryptoServiceProvider())
			{
				foreach (var store in stores)
				{
					try
					{
						store.Open(OpenFlags.ReadOnly);

						foreach (var current in store.Certificates)
						{
							var publicKey = current.PublicKey.EncodedKeyValue.RawData;
							var publicKeyHash = algorithm.ComputeHash(publicKey);

							if (publicKeyHash.SequenceEqual(keyHash))
							{
								certificate = current;

								return true;
							}
						}
					}
					finally
					{
						store.Close();
					}
				}
			}

			return false;
		}
	}
}
