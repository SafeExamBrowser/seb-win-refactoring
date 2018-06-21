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
		private RuntimeInfo runtimeInfo;

		public ISessionData CurrentSession { get; private set; }
		public Settings CurrentSettings { get; private set; }
		public string ReconfigurationFilePath { get; set; }

		public RuntimeInfo RuntimeInfo
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

		public ClientConfiguration BuildClientConfiguration()
		{
			return new ClientConfiguration
			{
				RuntimeInfo = RuntimeInfo,
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
				UpdateRuntimeInfo();
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

		private void InitializeRuntimeInfo()
		{
			var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(SafeExamBrowser));
			var executable = Assembly.GetEntryAssembly();
			var startTime = DateTime.Now;
			var logFolder = Path.Combine(appDataFolder, "Logs");
			var logFilePrefix = startTime.ToString("yyyy-MM-dd\\_HH\\hmm\\mss\\s");

			runtimeInfo = new RuntimeInfo();
			runtimeInfo.ApplicationStartTime = startTime;
			runtimeInfo.AppDataFolder = appDataFolder;
			runtimeInfo.BrowserCachePath = Path.Combine(appDataFolder, "Cache");
			runtimeInfo.BrowserLogFile = Path.Combine(logFolder, $"{logFilePrefix}_Browser.txt");
			runtimeInfo.ClientId = Guid.NewGuid();
			runtimeInfo.ClientAddress = $"{BASE_ADDRESS}/client/{Guid.NewGuid()}";
			runtimeInfo.ClientExecutablePath = Path.Combine(Path.GetDirectoryName(executable.Location), $"{nameof(SafeExamBrowser)}.Client.exe");
			runtimeInfo.ClientLogFile = Path.Combine(logFolder, $"{logFilePrefix}_Client.txt");
			runtimeInfo.ConfigurationFileExtension = ".seb";
			runtimeInfo.DefaultSettingsFileName = "SebClientSettings.seb";
			runtimeInfo.DownloadDirectory = Path.Combine(appDataFolder, "Downloads");
			runtimeInfo.ProgramCopyright = executable.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
			runtimeInfo.ProgramDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), nameof(SafeExamBrowser));
			runtimeInfo.ProgramTitle = executable.GetCustomAttribute<AssemblyTitleAttribute>().Title;
			runtimeInfo.ProgramVersion = executable.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
			runtimeInfo.RuntimeId = Guid.NewGuid();
			runtimeInfo.RuntimeAddress = $"{BASE_ADDRESS}/runtime/{Guid.NewGuid()}";
			runtimeInfo.RuntimeLogFile = Path.Combine(logFolder, $"{logFilePrefix}_Runtime.txt");
			runtimeInfo.ServiceAddress = $"{BASE_ADDRESS}/service";
		}

		private void UpdateRuntimeInfo()
		{
			RuntimeInfo.ClientId = Guid.NewGuid();
			RuntimeInfo.ClientAddress = $"{BASE_ADDRESS}/client/{Guid.NewGuid()}";
		}
	}
}
