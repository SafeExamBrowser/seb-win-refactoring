/*
 * Copyright (c) 2022 ETH Zürich, Educational Development and Technology (LET)
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
using SafeExamBrowser.Settings;

namespace SafeExamBrowser.Configuration.Cryptography
{
	public class KeyGenerator : IKeyGenerator
	{
		private readonly SHA256Managed algorithm;
		private readonly AppConfig appConfig;
		private readonly IIntegrityModule integrityModule;
		private readonly ILogger logger;
		private readonly AppSettings settings;

		private string browserExamKey;

		public KeyGenerator(AppConfig appConfig, IIntegrityModule integrityModule, ILogger logger, AppSettings settings)
		{
			this.algorithm = new SHA256Managed();
			this.appConfig = appConfig;
			this.integrityModule = integrityModule;
			this.logger = logger;
			this.settings = settings;
		}

		public string CalculateBrowserExamKeyHash(string url)
		{
			var urlWithoutFragment = url.Split('#')[0];
			var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(urlWithoutFragment + (browserExamKey ?? ComputeBrowserExamKey())));
			var key = ToString(hash);

			return key;
		}

		public string CalculateConfigurationKeyHash(string url)
		{
			var urlWithoutFragment = url.Split('#')[0];
			var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(urlWithoutFragment + settings.Browser.ConfigurationKey));
			var key = ToString(hash);

			return key;
		}

		private string ComputeBrowserExamKey()
		{
			var configurationKey = settings.Browser.ConfigurationKey;
			var salt = settings.Browser.BrowserExamKeySalt;

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
				logger.Debug("Successfully calculated BEK using integrity module.");
			}
			else
			{
				logger.Warn("Failed to calculate BEK using integrity module! Falling back to simplified calculation...");

				using (var algorithm = new HMACSHA256(salt))
				{
					var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(appConfig.CodeSignatureHash + appConfig.ProgramBuildVersion + configurationKey));
					var key = ToString(hash);

					browserExamKey = key;
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
