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
	internal class DisplayDataMapper : BaseDataMapper
	{
		internal override void Map(string key, object value, AppSettings settings)
		{
			switch (key)
			{
				case Keys.Display.AllowedDisplays:
					MapAllowedDisplays(settings, value);
					break;
				case Keys.Display.IgnoreError:
					MapIgnoreError(settings, value);
					break;
				case Keys.Display.InternalDisplayOnly:
					MapInternalDisplayOnly(settings, value);
					break;
			}
		}

		private void MapAllowedDisplays(AppSettings settings, object value)
		{
			if (value is int count)
			{
				settings.Display.AllowedDisplays = count;
			}
		}

		private void MapIgnoreError(AppSettings settings, object value)
		{
			if (value is bool ignore)
			{
				settings.Display.IgnoreError = ignore;
			}
		}

		private void MapInternalDisplayOnly(AppSettings settings, object value)
		{
			if (value is bool internalOnly)
			{
				settings.Display.InternalDisplayOnly = internalOnly;
			}
		}
	}
}
