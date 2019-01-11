/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
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

namespace SafeExamBrowser.Configuration.ConfigurationData
{
	internal class DataValues
	{
		private const string BASE_ADDRESS = "net.pipe://localhost/safeexambrowser";

		private AppConfig appConfig;
		private string executablePath;
		private string programCopyright;
		private string programTitle;
		private string programVersion;

		internal DataValues(string executablePath, string programCopyright, string programTitle, string programVersion)
		{
			this.executablePath = executablePath ?? string.Empty;
			this.programCopyright = programCopyright ?? string.Empty;
			this.programTitle = programTitle ?? string.Empty;
			this.programVersion = programVersion ?? string.Empty;
		}

		internal string GetAppDataFilePath()
		{
			return Path.Combine(appConfig.AppDataFolder, appConfig.DefaultSettingsFileName);
		}

		internal AppConfig InitializeAppConfig()
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

		internal ISessionConfiguration InitializeSessionConfiguration()
		{
			var configuration = new SessionConfiguration();

			appConfig.ClientId = Guid.NewGuid();
			appConfig.ClientAddress = $"{BASE_ADDRESS}/client/{Guid.NewGuid()}";

			configuration.AppConfig = appConfig.Clone();
			configuration.Id = Guid.NewGuid();
			configuration.StartupToken = Guid.NewGuid();

			return configuration;
		}

		internal Settings LoadDefaultSettings()
		{
			var settings = new Settings();

			settings.Browser.StartUrl = "https://www.safeexambrowser.org/start";
			settings.Browser.AllowAddressBar = false;
			settings.Browser.AllowBackwardNavigation = false;
			settings.Browser.AllowConfigurationDownloads = true;
			settings.Browser.AllowDeveloperConsole = false;
			settings.Browser.AllowDownloads = false;
			settings.Browser.AllowForwardNavigation = false;
			settings.Browser.AllowReloading = true;

			settings.Keyboard.AllowAltEsc = false;
			settings.Keyboard.AllowAltF4 = false;
			settings.Keyboard.AllowAltTab = true;
			settings.Keyboard.AllowCtrlEsc = false;
			settings.Keyboard.AllowEsc = true;
			settings.Keyboard.AllowF1 = true;
			settings.Keyboard.AllowF2 = true;
			settings.Keyboard.AllowF3 = true;
			settings.Keyboard.AllowF4 = true;
			settings.Keyboard.AllowF5 = true;
			settings.Keyboard.AllowF6 = true;
			settings.Keyboard.AllowF7 = true;
			settings.Keyboard.AllowF8 = true;
			settings.Keyboard.AllowF9 = true;
			settings.Keyboard.AllowF10 = true;
			settings.Keyboard.AllowF11 = true;
			settings.Keyboard.AllowF12 = true;
			settings.Keyboard.AllowPrintScreen = false;
			settings.Keyboard.AllowSystemKey = false;

			settings.KioskMode = KioskMode.CreateNewDesktop;

			settings.Mouse.AllowMiddleButton = false;
			settings.Mouse.AllowRightButton = true;

			settings.ServicePolicy = ServicePolicy.Optional;

			settings.Taskbar.AllowApplicationLog = false;
			settings.Taskbar.AllowKeyboardLayout = true;
			settings.Taskbar.AllowWirelessNetwork = false;
			settings.Taskbar.ShowClock = true;

			// TODO: Default values for alpha version only, remove for final release!
			settings.Browser.AllowAddressBar = true;
			settings.Browser.AllowBackwardNavigation = true;
			settings.Browser.AllowConfigurationDownloads = true;
			settings.Browser.AllowDeveloperConsole = true;
			settings.Browser.AllowDownloads = true;
			settings.Browser.AllowForwardNavigation = true;
			settings.Browser.AllowReloading = true;
			settings.KioskMode = KioskMode.None;
			settings.ServicePolicy = ServicePolicy.Optional;
			settings.Taskbar.AllowApplicationLog = true;
			settings.Taskbar.AllowWirelessNetwork = true;

			return settings;
		}
	}
}
