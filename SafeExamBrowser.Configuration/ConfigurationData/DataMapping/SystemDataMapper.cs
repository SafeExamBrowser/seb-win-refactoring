/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Settings;

namespace SafeExamBrowser.Configuration.ConfigurationData.DataMapping
{
	internal class SystemDataMapper : BaseDataMapper
	{
		internal override void Map(string key, object value, AppSettings settings)
		{
			switch (key)
			{
				case Keys.System.AlwaysOn:
					MapAlwaysOn(settings, value);
					break;
			}
		}

		private void MapAlwaysOn(AppSettings settings, object value)
		{
			if (value is bool alwaysOn)
			{
				settings.System.AlwaysOn = alwaysOn;
			}
		}
	}
}
