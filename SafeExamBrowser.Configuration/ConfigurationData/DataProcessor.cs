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
			AllowBrowserToolbarForReloading(rawData, settings);
			CalculateConfigurationKey(rawData, settings);
			HandleBrowserHomeFunctionality(settings);
			RemoveLegacyBrowsers(settings);
		}

		private void AllowBrowserToolbarForReloading(IDictionary<string, object> rawData, AppSettings settings)
		{
			var showReloadButton = rawData.TryGetValue(Keys.Browser.ShowReloadButton, out var v) && v is bool show && show;

			if (settings.Browser.AdditionalWindow.AllowReloading && showReloadButton)
			{
				settings.Browser.AdditionalWindow.ShowToolbar = true;
			}

			if (settings.Browser.MainWindow.AllowReloading && showReloadButton)
			{
				settings.Browser.MainWindow.ShowToolbar = true;
			}
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

		private void HandleBrowserHomeFunctionality(AppSettings settings)
		{
			settings.Browser.MainWindow.ShowHomeButton = settings.Browser.UseStartUrlAsHomeUrl || !string.IsNullOrWhiteSpace(settings.Browser.HomeUrl);
			settings.Browser.HomePasswordHash = settings.Security.QuitPasswordHash;
		}

		private void RemoveLegacyBrowsers(AppSettings settings)
		{
			var legacyBrowsers = new List<WhitelistApplication>();

			foreach (var application in settings.Applications.Whitelist)
			{
				var isEnginePath = application.ExecutablePath?.Contains("xulrunner") == true;
				var isFirefox = application.ExecutableName?.Equals("firefox.exe", StringComparison.OrdinalIgnoreCase) == true;
				var isXulRunner = application.ExecutableName?.Equals("xulrunner.exe", StringComparison.OrdinalIgnoreCase) == true;

				if (isEnginePath && (isFirefox || isXulRunner))
				{
					legacyBrowsers.Add(application);
				}
			}

			foreach (var legacyBrowser in legacyBrowsers)
			{
				settings.Applications.Whitelist.Remove(legacyBrowser);
			}
		}
	}
}
