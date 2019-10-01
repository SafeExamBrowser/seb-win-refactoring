/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Applications;

namespace SafeExamBrowser.Configuration.ConfigurationData
{
	internal partial class DataMapper
	{
		private void MapApplicationBlacklist(AppSettings settings, object value)
		{
			if (value is IList<object> applications)
			{
				foreach (var item in applications)
				{
					if (item is IDictionary<string, object> applicationData)
					{
						var isActive = applicationData.TryGetValue(Keys.Applications.ApplicationActive, out var v) && v is bool active && active;
						var isWindowsProcess = applicationData.TryGetValue(Keys.Applications.ApplicationOs, out v) && v is int os && os == Keys.WINDOWS;

						if (isActive && isWindowsProcess)
						{
							var application = new BlacklistApplication();

							if (applicationData.TryGetValue(Keys.Applications.ApplicationAutoTerminate, out v) && v is bool autoTerminate)
							{
								application.AutoTerminate = autoTerminate;
							}

							if (applicationData.TryGetValue(Keys.Applications.ApplicationExecutable, out v) && v is string executableName)
							{
								application.ExecutableName = executableName;
							}

							if (applicationData.TryGetValue(Keys.Applications.ApplicationOriginalName, out v) && v is string originalName)
							{
								application.ExecutableOriginalName = originalName;
							}

							settings.Applications.Blacklist.Add(application);
						}
					}
				}
			}
		}

		private void MapApplicationWhitelist(AppSettings settings, object value)
		{
			if (value is IList<object> applications)
			{
				foreach (var item in applications)
				{
					if (item is IDictionary<string, object> application)
					{
						var isActive = application.TryGetValue(Keys.Applications.ApplicationActive, out var v) && v is bool active && active;
						var isWindowsProcess = application.TryGetValue(Keys.Applications.ApplicationOs, out v) && v is int os && os == Keys.WINDOWS;

						if (isActive && isWindowsProcess)
						{

						}
					}
				}
			}
		}
	}
}
