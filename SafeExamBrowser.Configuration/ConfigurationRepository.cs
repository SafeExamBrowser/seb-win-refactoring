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
		private const string BASE_ADDRESS = "net.pipe://localhost/safeexambrowser";

		private bool firstSession = true;
		private AppConfig appConfig;

		public ISessionData CurrentSession { get; private set; }
		public Settings CurrentSettings { get; private set; }
		public string ReconfigurationFilePath { get; set; }

		public AppConfig AppConfig
		{
			get
			{
				if (appConfig == null)
				{
					InitializeAppConfig();
				}

				return appConfig;
			}
		}

		public ClientConfiguration BuildClientConfiguration()
		{
			return new ClientConfiguration
			{
				AppConfig = AppConfig,
				SessionId = CurrentSession.Id,
				Settings = CurrentSettings
			};
		}

		public void InitializeSessionConfiguration()
		{
			CurrentSession = new SessionData
			{
				Id = Guid.NewGuid(),
				StartupToken = Guid.NewGuid()
			};

			if (!firstSession)
			{
				UpdateAppConfig();
			}
			else
			{
				firstSession = false;
			}
		}

		public LoadStatus LoadSettings(Uri resource, string settingsPassword = null, string adminPassword = null)
		{
			// TODO: Implement loading mechanism

			LoadDefaultSettings();

			return LoadStatus.Success;
		}

		public void LoadDefaultSettings()
		{
			// TODO: Implement default settings

			CurrentSettings = new Settings();

			CurrentSettings.KioskMode = KioskMode.CreateNewDesktop;
			CurrentSettings.ServicePolicy = ServicePolicy.Optional;

			CurrentSettings.Browser.StartUrl = "https://www.safeexambrowser.org/testing";
			CurrentSettings.Browser.AllowAddressBar = true;
			CurrentSettings.Browser.AllowBackwardNavigation = true;
			CurrentSettings.Browser.AllowDeveloperConsole = true;
			CurrentSettings.Browser.AllowForwardNavigation = true;
			CurrentSettings.Browser.AllowReloading = true;
			CurrentSettings.Browser.AllowDownloads = true;

			CurrentSettings.Taskbar.AllowApplicationLog = true;
			CurrentSettings.Taskbar.AllowKeyboardLayout = true;
			CurrentSettings.Taskbar.AllowWirelessNetwork = true;
		}

		private void InitializeAppConfig()
		{
			var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(SafeExamBrowser));
			var executable = Assembly.GetEntryAssembly();
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
			appConfig.ClientExecutablePath = Path.Combine(Path.GetDirectoryName(executable.Location), $"{nameof(SafeExamBrowser)}.Client.exe");
			appConfig.ClientLogFile = Path.Combine(logFolder, $"{logFilePrefix}_Client.log");
			appConfig.ConfigurationFileExtension = ".seb";
			appConfig.DefaultSettingsFileName = "SebClientSettings.seb";
			appConfig.DownloadDirectory = Path.Combine(appDataFolder, "Downloads");
			appConfig.ProgramCopyright = executable.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
			appConfig.ProgramDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), nameof(SafeExamBrowser));
			appConfig.ProgramTitle = executable.GetCustomAttribute<AssemblyTitleAttribute>().Title;
			appConfig.ProgramVersion = executable.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
			appConfig.RuntimeId = Guid.NewGuid();
			appConfig.RuntimeAddress = $"{BASE_ADDRESS}/runtime/{Guid.NewGuid()}";
			appConfig.RuntimeLogFile = Path.Combine(logFolder, $"{logFilePrefix}_Runtime.log");
			appConfig.SebUriScheme = "seb";
			appConfig.SebUriSchemeSecure = "sebs";
			appConfig.ServiceAddress = $"{BASE_ADDRESS}/service";
		}

		private void UpdateAppConfig()
		{
			AppConfig.ClientId = Guid.NewGuid();
			AppConfig.ClientAddress = $"{BASE_ADDRESS}/client/{Guid.NewGuid()}";
		}
	}
}
