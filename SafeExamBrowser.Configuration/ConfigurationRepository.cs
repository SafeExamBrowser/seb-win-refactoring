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
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;

namespace SafeExamBrowser.Configuration
{
	public class ConfigurationRepository : IConfigurationRepository
	{
		private RuntimeInfo runtimeInfo;

		public ISettings CurrentSettings { get; private set; }

		public IRuntimeInfo RuntimeInfo
		{
			get
			{
				if (runtimeInfo == null)
				{
					InitializeRuntimeInfo();
				}

				return runtimeInfo;
			}
		}

		public ISettings LoadSettings(Uri path)
		{
			// TODO

			return LoadDefaultSettings();
		}

		public ISettings LoadDefaultSettings()
		{
			var settings = new Settings.Settings();

			// TODO
			settings.ServicePolicy = ServicePolicy.Optional;

			CurrentSettings = settings;

			return settings;
		}

		private void InitializeRuntimeInfo()
		{
			var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(SafeExamBrowser));
			var baseAddress = "net.pipe://localhost/safeexambrowser";
			var clientId = Guid.NewGuid();
			var executable = Assembly.GetEntryAssembly();
			var runtimeId = Guid.NewGuid();
			var startTime = DateTime.Now;
			var logFolder = Path.Combine(appDataFolder, "Logs");
			var logFilePrefix = startTime.ToString("yyyy-MM-dd\\_HH\\hmm\\mss\\s");

			runtimeInfo = new RuntimeInfo
			{
				ApplicationStartTime = startTime,
				AppDataFolder = appDataFolder,
				BrowserCachePath = Path.Combine(appDataFolder, "Cache"),
				BrowserLogFile = Path.Combine(logFolder, $"{logFilePrefix}_Browser.txt"),
				ClientId = Guid.NewGuid(),
				ClientAddress = $"{baseAddress}/client/{clientId}",
				ClientLogFile = Path.Combine(logFolder, $"{logFilePrefix}_Client.txt"),
				DefaultSettingsFileName = "SebClientSettings.seb",
				ProgramCopyright = executable.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright,
				ProgramDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), nameof(SafeExamBrowser)),
				ProgramTitle = executable.GetCustomAttribute<AssemblyTitleAttribute>().Title,
				ProgramVersion = executable.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion,
				RuntimeId = Guid.NewGuid(),
				RuntimeAddress = $"{baseAddress}/runtime/{runtimeId}",
				RuntimeLogFile = Path.Combine(logFolder, $"{logFilePrefix}_Runtime.txt"),
				ServiceAddress = $"{baseAddress}/service"
			};
		}
	}
}
