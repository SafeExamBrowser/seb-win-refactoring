/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Contracts.Configuration.Settings;

namespace SafeExamBrowser.Configuration.ConfigurationData
{
	internal class DataMapper
	{
		internal void MapRawDataToSettings(IDictionary<string, object> rawData, Settings settings)
		{
			foreach (var item in rawData)
			{
				Map(item.Key, item.Value, settings);
			}
		}

		private void Map(string key, object value, Settings settings)
		{
			switch (key)
			{
				case Keys.General.AdminPasswordHash:
					MapAdminPasswordHash(settings, value);
					break;
				case Keys.General.StartUrl:
					MapStartUrl(settings, value);
					break;
				case Keys.ConfigurationFile.ConfigurationPurpose:
					MapConfigurationMode(settings, value);
					break;
			}
		}

		private void MapAdminPasswordHash(Settings settings, object value)
		{
			if (value is string hash)
			{
				settings.AdminPasswordHash = hash;
			}
		}

		private void MapConfigurationMode(Settings settings, object value)
		{
			if (value is int mode)
			{
				settings.ConfigurationMode = mode == 1 ? ConfigurationMode.ConfigureClient : ConfigurationMode.Exam;
			}
		}

		private void MapStartUrl(Settings settings, object value)
		{
			if (value is string url)
			{
				settings.Browser.StartUrl = url;
			}
		}
	}
}
