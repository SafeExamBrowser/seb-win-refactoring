/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Applications;

namespace SafeExamBrowser.Configuration.ConfigurationData.DataMapping
{
	internal class ApplicationDataMapper : BaseDataMapper
	{
		internal override void Map(string key, object value, AppSettings settings)
		{
			switch (key)
			{
				case Keys.Applications.Blacklist:
					MapApplicationBlacklist(settings, value);
					break;
				case Keys.Applications.Whitelist:
					MapApplicationWhitelist(settings, value);
					break;
			}
		}

		private void MapApplicationBlacklist(AppSettings settings, object value)
		{
			if (value is IList<object> applications)
			{
				foreach (var item in applications)
				{
					if (item is IDictionary<string, object> applicationData)
					{
						var isWindowsProcess = applicationData.TryGetValue(Keys.Applications.OperatingSystem, out var v) && v is int os && os == Keys.WINDOWS;

						if (isWindowsProcess)
						{
							var application = new BlacklistApplication();
							var isActive = applicationData.TryGetValue(Keys.Applications.Active, out v) && v is bool active && active;

							if (applicationData.TryGetValue(Keys.Applications.AutoTerminate, out v) && v is bool autoTerminate)
							{
								application.AutoTerminate = autoTerminate;
							}

							if (applicationData.TryGetValue(Keys.Applications.ExecutableName, out v) && v is string executableName)
							{
								application.ExecutableName = executableName;
							}

							if (applicationData.TryGetValue(Keys.Applications.OriginalName, out v) && v is string originalName)
							{
								application.OriginalName = originalName;
							}

							var defaultEntry = settings.Applications.Blacklist.FirstOrDefault(a =>
							{
								return a.ExecutableName?.Equals(application.ExecutableName, StringComparison.OrdinalIgnoreCase) == true
									&& a.OriginalName?.Equals(application.OriginalName, StringComparison.OrdinalIgnoreCase) == true;
							});

							if (defaultEntry != default(BlacklistApplication))
							{
								settings.Applications.Blacklist.Remove(defaultEntry);
							}

							if (isActive)
							{
								settings.Applications.Blacklist.Add(application);
							}
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
					if (item is IDictionary<string, object> applicationData)
					{
						var isActive = applicationData.TryGetValue(Keys.Applications.Active, out var v) && v is bool active && active;
						var isWindowsProcess = applicationData.TryGetValue(Keys.Applications.OperatingSystem, out v) && v is int os && os == Keys.WINDOWS;

						if (isActive && isWindowsProcess)
						{
							var application = new WhitelistApplication();

							if (applicationData.TryGetValue(Keys.Applications.AllowCustomPath, out v) && v is bool allowCustomPath)
							{
								application.AllowCustomPath = allowCustomPath;
							}

							if (applicationData.TryGetValue(Keys.Applications.AllowRunning, out v) && v is bool allowRunning)
							{
								application.AllowRunning = allowRunning;
							}

							if (applicationData.TryGetValue(Keys.Applications.Arguments, out v) && v is IList<object> arguments)
							{
								foreach (var argumentItem in arguments)
								{
									if (argumentItem is IDictionary<string, object> argumentData)
									{
										var argActive = argumentData.TryGetValue(Keys.Applications.Active, out v) && v is bool a && a;

										if (argActive && argumentData.TryGetValue(Keys.Applications.Argument, out v) && v is string argument)
										{
											application.Arguments.Add(argument);
										}
									}
								}
							}

							if (applicationData.TryGetValue(Keys.Applications.AutoStart, out v) && v is bool autoStart)
							{
								application.AutoStart = autoStart;
							}

							if (applicationData.TryGetValue(Keys.Applications.AutoTerminate, out v) && v is bool autoTerminate)
							{
								application.AutoTerminate = autoTerminate;
							}

							if (applicationData.TryGetValue(Keys.Applications.Description, out v) && v is string description)
							{
								application.Description = description;
							}

							if (applicationData.TryGetValue(Keys.Applications.DisplayName, out v) && v is string displayName)
							{
								application.DisplayName = displayName;
							}

							if (applicationData.TryGetValue(Keys.Applications.ExecutableName, out v) && v is string executableName)
							{
								application.ExecutableName = executableName;
							}

							if (applicationData.TryGetValue(Keys.Applications.ExecutablePath, out v) && v is string executablePath)
							{
								application.ExecutablePath = executablePath;
							}

							if (applicationData.TryGetValue(Keys.Applications.OriginalName, out v) && v is string originalName)
							{
								application.OriginalName = originalName;
							}

							if (applicationData.TryGetValue(Keys.Applications.ShowInShell, out v) && v is bool showInShell)
							{
								application.ShowInShell = showInShell;
							}

							settings.Applications.Whitelist.Add(application);
						}
					}
				}
			}
		}
	}
}
