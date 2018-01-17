/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Reflection;
using SafeExamBrowser.Contracts.Configuration.Settings;

namespace SafeExamBrowser.Configuration.Settings
{
	public class SettingsRepository : ISettingsRepository
	{
		public ISettings Load(Uri path)
		{
			// TODO
			throw new NotImplementedException();
		}

		public ISettings LoadDefaults()
		{
			var browser = new BrowserSettings();
			var keyboard = new KeyboardSettings();
			var logging = new LoggingSettings();
			var mouse = new MouseSettings();
			var taskbar = new TaskbarSettings();
			var settings = new Settings(browser, keyboard, logging, mouse, taskbar);
			var executable = Assembly.GetEntryAssembly();
			var startTime = DateTime.Now;
			var logFolderName = "Logs";
			var logFilePrefix = startTime.ToString("yyyy-MM-dd\\_HH\\hmm\\mss\\s");

			settings.AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(SafeExamBrowser));
			settings.ProgramCopyright = executable.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
			settings.ProgramTitle = executable.GetCustomAttribute<AssemblyTitleAttribute>().Title;
			settings.ProgramVersion = executable.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

			browser.CachePath = Path.Combine(settings.AppDataFolder, "Cache");

			logging.ApplicationStartTime = DateTime.Now;
			logging.BrowserLogFile = Path.Combine(settings.AppDataFolder, logFolderName, $"{logFilePrefix}_Browser.txt");
			logging.ClientLogFile = Path.Combine(settings.AppDataFolder, logFolderName, $"{logFilePrefix}_Client.txt");
			logging.RuntimeLogFile = Path.Combine(settings.AppDataFolder, logFolderName, $"{logFilePrefix}_Runtime.txt");

			return settings;
		}
	}
}
