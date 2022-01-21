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
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings;

namespace SafeExamBrowser.Configuration.Cryptography
{
	public class KeyGenerator : IKeyGenerator
	{
		private readonly SHA256Managed algorithm;
		private readonly AppConfig appConfig;
		private readonly ILogger logger;
		private readonly AppSettings settings;

		private string browserExamKey;

		public KeyGenerator(AppConfig appConfig, ILogger logger, AppSettings settings)
		{
			this.algorithm = new SHA256Managed();
			this.appConfig = appConfig;
			this.logger = logger;
			this.settings = settings;
		}

		public string CalculateBrowserExamKeyHash(string url)
		{
			var urlWithoutFragment = url.Split('#')[0];
			var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(urlWithoutFragment + (browserExamKey ?? ComputeBrowserExamKey())));
			var key = BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);

			return key;
		}

		public string CalculateConfigurationKeyHash(string url)
		{
			var urlWithoutFragment = url.Split('#')[0];
			var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(urlWithoutFragment + settings.Browser.ConfigurationKey));
			var key = BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);

			return key;
		}

		private string ComputeBrowserExamKey()
		{
			var salt = settings.Browser.BrowserExamKeySalt;

			if (salt == default(byte[]))
			{
				salt = new byte[0];
				logger.Warn("The current configuration does not contain a salt value for the browser exam key!");
			}

			using (var algorithm = new HMACSHA256(salt))
			{
				var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(appConfig.CodeSignatureHash + appConfig.ProgramBuildVersion + settings.Browser.ConfigurationKey));
				var key = BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);

				browserExamKey = key;

				return browserExamKey;
			}
		}
	}
}
