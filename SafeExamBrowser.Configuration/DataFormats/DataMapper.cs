/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Contracts.Configuration.Settings;

namespace SafeExamBrowser.Configuration.DataFormats
{
	internal static class DataMapper
	{
		internal static void MapTo(this Dictionary<string, object> rawData, Settings settings)
		{
			foreach (var kvp in rawData)
			{
				Map(kvp.Key, kvp.Value, settings);
			}
		}

		private static void Map(string key, object value, Settings settings)
		{
			switch (key)
			{
				case "sebConfigPurpose":
					settings.MapConfigurationMode(value);
					break;
				case "startURL":
					settings.MapStartUrl(value);
					break;
			}
		}

		private static void MapConfigurationMode(this Settings settings, object value)
		{
			if (value is Int32 mode)
			{
				settings.ConfigurationMode = mode == 1 ? ConfigurationMode.ConfigureClient : ConfigurationMode.Exam;
			}
		}

		private static void MapStartUrl(this Settings settings, object value)
		{
			if (value is string startUrl)
			{
				settings.Browser.StartUrl = startUrl;
			}
		}
	}
}
