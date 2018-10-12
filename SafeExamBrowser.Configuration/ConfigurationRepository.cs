/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Configuration
{
	public class ConfigurationRepository : IConfigurationRepository
	{
		private const string BASE_ADDRESS = "net.pipe://localhost/safeexambrowser";

		private readonly string executablePath;
		private readonly string programCopyright;
		private readonly string programTitle;
		private readonly string programVersion;

		private AppConfig appConfig;

		public ConfigurationRepository(string executablePath, string programCopyright, string programTitle, string programVersion)
		{
			this.executablePath = executablePath ?? string.Empty;
			this.programCopyright = programCopyright ?? string.Empty;
			this.programTitle = programTitle ?? string.Empty;
			this.programVersion = programVersion ?? string.Empty;
		}

		public AppConfig InitializeAppConfig()
		{
			var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(SafeExamBrowser));
			var startTime = DateTime.Now;
			var logFolder = Path.Combine(appDataFolder, "Logs");
			var logFilePrefix = startTime.ToString("yyyy-MM-dd\\_HH\\hmm\\mss\\s");

			appConfig = new AppConfig();
			appConfig.ApplicationStartTime = startTime;
			appConfig.AppDataFolder = appDataFolder;
			appConfig.BrowserCachePath = Path.Combine(appDataFolder, "Cache");
			appConfig.BrowserLogFile = Path.Combine(logFolder, $"{logFilePrefix}_Browser.log");
			appConfig.ClientId = Guid.NewGuid();
			appConfig.ClientAddress = $"{BASE_ADDRESS}/client/{Guid.NewGuid()}";
			appConfig.ClientExecutablePath = Path.Combine(Path.GetDirectoryName(executablePath), $"{nameof(SafeExamBrowser)}.Client.exe");
			appConfig.ClientLogFile = Path.Combine(logFolder, $"{logFilePrefix}_Client.log");
			appConfig.ConfigurationFileExtension = ".seb";
			appConfig.DefaultSettingsFileName = "SebClientSettings.seb";
			appConfig.DownloadDirectory = Path.Combine(appDataFolder, "Downloads");
			appConfig.LogLevel = LogLevel.Debug;
			appConfig.ProgramCopyright = programCopyright;
			appConfig.ProgramDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), nameof(SafeExamBrowser));
			appConfig.ProgramTitle = programTitle;
			appConfig.ProgramVersion = programVersion;
			appConfig.RuntimeId = Guid.NewGuid();
			appConfig.RuntimeAddress = $"{BASE_ADDRESS}/runtime/{Guid.NewGuid()}";
			appConfig.RuntimeLogFile = Path.Combine(logFolder, $"{logFilePrefix}_Runtime.log");
			appConfig.SebUriScheme = "seb";
			appConfig.SebUriSchemeSecure = "sebs";
			appConfig.ServiceAddress = $"{BASE_ADDRESS}/service";

			return appConfig;
		}

		public ISessionConfiguration InitializeSessionConfiguration()
		{
			var configuration = new SessionConfiguration();

			UpdateAppConfig();

			configuration.AppConfig = CloneAppConfig();
			configuration.Id = Guid.NewGuid();
			configuration.StartupToken = Guid.NewGuid();

			return configuration;
		}

		public LoadStatus TryLoadSettings(Uri resource, out Settings settings, string adminPassword = null, string settingsPassword = null)
		{
			// TODO: Implement loading mechanism

			settings = LoadDefaultSettings();

			return LoadStatus.Success;
		}

		public Settings LoadDefaultSettings()
		{
			// TODO: Implement default settings

			var settings = new Settings();

			settings.KioskMode = new Random().Next(10) < 5 ? KioskMode.CreateNewDesktop : KioskMode.DisableExplorerShell;
			settings.ServicePolicy = ServicePolicy.Optional;

			settings.Browser.StartUrl = "https://www.safeexambrowser.org/testing";
			settings.Browser.AllowAddressBar = true;
			settings.Browser.AllowBackwardNavigation = true;
			settings.Browser.AllowDeveloperConsole = true;
			settings.Browser.AllowForwardNavigation = true;
			settings.Browser.AllowReloading = true;
			settings.Browser.AllowDownloads = true;

			settings.Taskbar.AllowApplicationLog = true;
			settings.Taskbar.AllowKeyboardLayout = true;
			settings.Taskbar.AllowWirelessNetwork = true;

			return settings;
		}

		private AppConfig CloneAppConfig()
		{
			return new AppConfig
			{
				AppDataFolder = appConfig.AppDataFolder,
				ApplicationStartTime = appConfig.ApplicationStartTime,
				BrowserCachePath = appConfig.BrowserCachePath,
				BrowserLogFile = appConfig.BrowserLogFile,
				ClientAddress = appConfig.ClientAddress,
				ClientExecutablePath = appConfig.ClientExecutablePath,
				ClientId = appConfig.ClientId,
				ClientLogFile = appConfig.ClientLogFile,
				ConfigurationFileExtension = appConfig.ConfigurationFileExtension,
				DefaultSettingsFileName = appConfig.DefaultSettingsFileName,
				DownloadDirectory = appConfig.DownloadDirectory,
				LogLevel = appConfig.LogLevel,
				ProgramCopyright = appConfig.ProgramCopyright,
				ProgramDataFolder = appConfig.ProgramDataFolder,
				ProgramTitle = appConfig.ProgramTitle,
				ProgramVersion = appConfig.ProgramVersion,
				RuntimeAddress = appConfig.RuntimeAddress,
				RuntimeId = appConfig.RuntimeId,
				RuntimeLogFile = appConfig.RuntimeLogFile,
				SebUriScheme = appConfig.SebUriScheme,
				SebUriSchemeSecure = appConfig.SebUriSchemeSecure,
				ServiceAddress = appConfig.ServiceAddress
			};
		}

		private void UpdateAppConfig()
		{
			appConfig.ClientId = Guid.NewGuid();
			appConfig.ClientAddress = $"{BASE_ADDRESS}/client/{Guid.NewGuid()}";
		}
	}
}
