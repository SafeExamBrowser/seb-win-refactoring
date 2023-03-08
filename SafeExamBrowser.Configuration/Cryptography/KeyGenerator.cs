/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Security.Cryptography;
using System.Text;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Configuration.Contracts.Integrity;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Configuration.Cryptography
{
	public class KeyGenerator : IKeyGenerator
	{
		private readonly object @lock = new object();

		private readonly SHA256Managed algorithm;
		private readonly AppConfig appConfig;
		private readonly IIntegrityModule integrityModule;
		private readonly ILogger logger;

		private string browserExamKey;

		public KeyGenerator(AppConfig appConfig, IIntegrityModule integrityModule, ILogger logger)
		{
			this.algorithm = new SHA256Managed();
			this.appConfig = appConfig;
			this.integrityModule = integrityModule;
			this.logger = logger;
		}

		public string CalculateAppSignatureKey(string connectionToken, string salt)
		{
			if (integrityModule.TryCalculateAppSignatureKey(connectionToken, salt, out var appSignatureKey))
			{
				logger.Debug("Successfully calculated app signature key using integrity module.");
			}
			else
			{
				logger.Error("Failed to calculate app signature key using integrity module!");
			}

			return appSignatureKey;
		}

		public string CalculateBrowserExamKeyHash(string configurationKey, byte[] salt, string url)
		{
			var urlWithoutFragment = url.Split('#')[0];
			var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(urlWithoutFragment + (browserExamKey ?? ComputeBrowserExamKey(configurationKey, salt))));
			var key = ToString(hash);

			return key;
		}

		public string CalculateConfigurationKeyHash(string configurationKey, string url)
		{
			var urlWithoutFragment = url.Split('#')[0];
			var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(urlWithoutFragment + configurationKey));
			var key = ToString(hash);

			return key;
		}

		public void UseCustomBrowserExamKey(string browserExamKey)
		{
			if (browserExamKey != default)
			{
				this.browserExamKey = browserExamKey;
				logger.Debug("Initialized custom browser exam key.");
			}
		}

		private string ComputeBrowserExamKey(string configurationKey, byte[] salt)
		{
			lock (@lock)
			{
				if (browserExamKey == default)
				{
					logger.Debug("Initializing browser exam key...");

					if (configurationKey == default)
					{
						configurationKey = "";
						logger.Warn("The current configuration does not contain a value for the configuration key!");
					}

					if (salt == default || salt.Length == 0)
					{
						salt = new byte[0];
						logger.Warn("The current configuration does not contain a salt value for the browser exam key!");
					}

					if (integrityModule.TryCalculateBrowserExamKey(configurationKey, ToString(salt), out browserExamKey))
					{
						logger.Debug("Successfully calculated browser exam key using integrity module.");
					}
					else
					{
						logger.Warn("Failed to calculate browser exam key using integrity module! Falling back to simplified calculation...");

						using (var algorithm = new HMACSHA256(salt))
						{
							var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(appConfig.CodeSignatureHash + appConfig.ProgramBuildVersion + configurationKey));
							var key = ToString(hash);

							browserExamKey = key;
						}
					}
				}
			}

			return browserExamKey;
		}

		private string ToString(byte[] bytes)
		{
			return BitConverter.ToString(bytes).ToLower().Replace("-", string.Empty);
		}
	}
}
