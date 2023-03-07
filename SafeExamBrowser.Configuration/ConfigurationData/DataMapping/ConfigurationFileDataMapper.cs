/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Settings;

namespace SafeExamBrowser.Configuration.ConfigurationData.DataMapping
{
	internal class ConfigurationFileDataMapper : BaseDataMapper
	{
		internal override void Map(string key, object value, AppSettings settings)
		{
			switch (key)
			{
				case Keys.ConfigurationFile.ConfigurationPurpose:
					MapConfigurationMode(settings, value);
					break;
				case Keys.ConfigurationFile.SessionMode:
					MapSessionMode(settings, value);
					break;
			}
		}

		private void MapConfigurationMode(AppSettings settings, object value)
		{
			const int CONFIGURE_CLIENT = 1;

			if (value is int mode)
			{
				settings.ConfigurationMode = mode == CONFIGURE_CLIENT ? ConfigurationMode.ConfigureClient : ConfigurationMode.Exam;
			}
		}

		private void MapSessionMode(AppSettings settings, object value)
		{
			const int SERVER = 1;

			if (value is int mode)
			{
				settings.SessionMode = mode == SERVER ? SessionMode.Server : SessionMode.Normal;
			}
		}
	}
}
