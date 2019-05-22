/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Configuration.ConfigurationData
{
	internal partial class DataMapper
	{
		private void MapAdminPasswordHash(Settings settings, object value)
		{
			if (value is string hash)
			{
				settings.AdminPasswordHash = hash;
			}
		}

		private void MapApplicationLogAccess(IDictionary<string, object> rawData, Settings settings)
		{
			var hasValue = rawData.TryGetValue(Keys.General.AllowApplicationLog, out var value);

			if (hasValue && value is bool allow)
			{
				settings.AllowApplicationLogAccess = allow;
			}

			if (settings.AllowApplicationLogAccess)
			{
				settings.ActionCenter.ShowApplicationLog = true;
			}
			else
			{
				settings.ActionCenter.ShowApplicationLog = false;
				settings.Taskbar.ShowApplicationLog = false;
			}
		}

		private void MapLogLevel(Settings settings, object value)
		{
			const int ERROR = 0, WARNING = 1, INFO = 2;

			if (value is int level)
			{
				settings.LogLevel = level == ERROR ? LogLevel.Error : (level == WARNING ? LogLevel.Warning : (level == INFO ? LogLevel.Info : LogLevel.Debug));
			}
		}

		private void MapQuitPasswordHash(Settings settings, object value)
		{
			if (value is string hash)
			{
				settings.QuitPasswordHash = hash;
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
