/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using SafeExamBrowser.Configuration.ConfigurationData;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Configuration.Cryptography
{
	public class CertificateStore : ICertificateStore
	{
		private ILogger logger;

		private readonly X509Store[] stores = new[]
		{
			new X509Store(StoreLocation.CurrentUser),
			new X509Store(StoreLocation.LocalMachine),
			new X509Store(StoreName.TrustedPeople)
		};

		public CertificateStore(ILogger logger)
		{
			this.logger = logger;
		}

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

		public void ExtractAndImportIdentities(IDictionary<string, object> data)
		{
			const int IDENTITY_CERTIFICATE = 1;
			var hasCertificates = data.TryGetValue(Keys.Network.Certificates.EmbeddedCertificates, out var value);

			if (hasCertificates && value is IList<IDictionary<string, object>> certificates)
			{
				var toRemove = new List<IDictionary<string, object>>();

				foreach (var certificate in certificates)
				{
					var hasData = certificate.TryGetValue(Keys.Network.Certificates.CertificateData, out var dataValue);
					var hasType = certificate.TryGetValue(Keys.Network.Certificates.CertificateType, out var typeValue);
					var isIdentity = typeValue is int type && type == IDENTITY_CERTIFICATE;

					if (hasData && hasType && isIdentity && dataValue is byte[] certificateData)
					{
						ImportIdentityCertificate(certificateData, new X509Store(StoreLocation.CurrentUser));
						ImportIdentityCertificate(certificateData, new X509Store(StoreName.TrustedPeople, StoreLocation.LocalMachine));

						toRemove.Add(certificate);
					}
				}

				toRemove.ForEach(c => certificates.Remove(c));
			}
		}

		private void ImportIdentityCertificate(byte[] certificateData, X509Store store)
		{
			try
			{
				var certificate = new X509Certificate2();

				certificate.Import(certificateData, "Di𝈭l𝈖Ch𝈒ah𝉇t𝈁a𝉈Hai1972", X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.PersistKeySet);

				store.Open(OpenFlags.ReadWrite);
				store.Add(certificate);

				logger.Info($"Successfully imported identity certificate into {store.Location}.{store.Name}.");
			}
			catch (Exception e)
			{
				logger.Error($"Failed to import identity certificate into {store.Location}.{store.Name}!", e);
			}
			finally
			{
				store.Close();
			}
		}
	}
}
