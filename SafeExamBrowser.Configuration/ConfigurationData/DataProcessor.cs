/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Applications;

namespace SafeExamBrowser.Configuration.ConfigurationData
{
	internal class DataProcessor
	{
		internal void Process(IDictionary<string, object> rawData, AppSettings settings)
		{
			AllowReconfiguration(settings);
			CalculateConfigurationKey(rawData, settings);
			RemoveLegacyBrowser(settings);
		}

		private void AllowReconfiguration(AppSettings settings)
		{
			settings.Security.AllowReconfiguration = settings.ConfigurationMode == ConfigurationMode.ConfigureClient;
		}

		private void CalculateConfigurationKey(IDictionary<string, object> rawData, AppSettings settings)
		{
			using (var algorithm = new SHA256Managed())
			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream))
			{
				Json.Serialize(rawData, writer);

				writer.Flush();
				stream.Seek(0, SeekOrigin.Begin);

				var hash = algorithm.ComputeHash(stream);
				var key = BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);

				settings.Browser.ConfigurationKey = key;
			}
		}

		private void RemoveLegacyBrowser(AppSettings settings)
		{
			var legacyBrowser = default(WhitelistApplication);

			foreach (var application in settings.Applications.Whitelist)
			{
				var isEnginePath = application.ExecutablePath?.Contains("xulrunner") == true;
				var isFirefox = application.ExecutableName?.Equals("firefox.exe", StringComparison.OrdinalIgnoreCase) == true;
				var isXulRunner = application.ExecutableName?.Equals("xulrunner.exe", StringComparison.OrdinalIgnoreCase) == true;

				if (isEnginePath && (isFirefox || isXulRunner))
				{
					legacyBrowser = application;
				}
			}

			settings.Applications.Whitelist.Remove(legacyBrowser);
		}
	}
}
