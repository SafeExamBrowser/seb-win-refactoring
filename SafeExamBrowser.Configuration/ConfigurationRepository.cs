/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SafeExamBrowser.Configuration.DataFormats;
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
		private IHashAlgorithm hashAlgorithm;
		private IList<IDataFormat> dataFormats;
		private IList<IResourceLoader> resourceLoaders;
		private ILogger logger;

		public ConfigurationRepository(
			IHashAlgorithm hashAlgorithm,
			ILogger logger,
			string executablePath,
			string programCopyright,
			string programTitle,
			string programVersion)
		{
			dataFormats = new List<IDataFormat>();
			resourceLoaders = new List<IResourceLoader>();

			this.hashAlgorithm = hashAlgorithm;
			this.logger = logger;
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

			configuration.AppConfig = appConfig.Clone();
			configuration.Id = Guid.NewGuid();
			configuration.StartupToken = Guid.NewGuid();

			return configuration;
		}

		public Settings LoadDefaultSettings()
		{
			var settings = new Settings();

			// TODO: Specify default settings

			settings.KioskMode = KioskMode.None;
			settings.ServicePolicy = ServicePolicy.Optional;

			settings.Browser.StartUrl = "https://www.safeexambrowser.org/testing";
			settings.Browser.AllowAddressBar = true;
			settings.Browser.AllowBackwardNavigation = true;
			settings.Browser.AllowConfigurationDownloads = true;
			settings.Browser.AllowDeveloperConsole = true;
			settings.Browser.AllowDownloads = true;
			settings.Browser.AllowForwardNavigation = true;
			settings.Browser.AllowReloading = true;

			settings.Taskbar.AllowApplicationLog = true;
			settings.Taskbar.AllowKeyboardLayout = true;
			settings.Taskbar.AllowWirelessNetwork = true;

			return settings;
		}

		public void Register(IDataFormat dataFormat)
		{
			dataFormats.Add(dataFormat);
		}

		public void Register(IResourceLoader resourceLoader)
		{
			resourceLoaders.Add(resourceLoader);
		}

		public LoadStatus TryLoadSettings(Uri resource, PasswordInfo passwordInfo, out Settings settings)
		{
			logger.Info($"Attempting to load '{resource}'...");

			settings = LoadDefaultSettings();

			try
			{
				var status = TryLoadData(resource, out Stream data);

				using (data)
				{
					if (status == LoadStatus.LoadWithBrowser)
					{
						return HandleBrowserResource(resource, settings);
					}

					if (status != LoadStatus.Success)
					{
						return status;
					}

					return TryParseData(data, passwordInfo, resource, settings);
				}
			}
			catch (Exception e)
			{
				logger.Error($"Unexpected error while trying to load '{resource}'!", e);

				return LoadStatus.UnexpectedError;
			}
		}

		private void ExtractAndImportCertificates(IDictionary<string, object> data)
		{
			// TODO
		}

		private LoadStatus HandleBrowserResource(Uri resource, Settings settings)
		{
			settings.Browser.StartUrl = resource.AbsoluteUri;
			logger.Info($"The resource needs authentication or is HTML data, loaded default settings with '{resource}' as startup URL.");

			return LoadStatus.Success;
		}

		private void HandleParseSuccess(ParseResult result, Settings settings, PasswordInfo passwordInfo, Uri resource)
		{
			var appDataFile = new Uri(Path.Combine(appConfig.AppDataFolder, appConfig.DefaultSettingsFileName));
			var programDataFile = new Uri(Path.Combine(appConfig.ProgramDataFolder, appConfig.DefaultSettingsFileName));
			var isAppDataFile = resource.AbsolutePath.Equals(appDataFile.AbsolutePath, StringComparison.OrdinalIgnoreCase);
			var isProgramDataFile = resource.AbsolutePath.Equals(programDataFile.AbsolutePath, StringComparison.OrdinalIgnoreCase);

			logger.Info("Mapping settings data...");
			result.RawData.MapTo(settings);

			if (settings.ConfigurationMode == ConfigurationMode.ConfigureClient && !isAppDataFile && !isProgramDataFile)
			{
				result.Status = TryConfigureClient(result.RawData, settings, passwordInfo);
			}
		}

		private LoadStatus TryLoadData(Uri resource, out Stream data)
		{
			var status = LoadStatus.NotSupported;
			var resourceLoader = resourceLoaders.FirstOrDefault(l => l.CanLoad(resource));

			data = default(Stream);

			if (resourceLoader != null)
			{
				status = resourceLoader.TryLoad(resource, out data);
				logger.Info($"Tried to load data from '{resource}' using {resourceLoader.GetType().Name} -> Result: {status}.");
			}
			else
			{
				logger.Warn($"No resource loader found for '{resource}'!");
			}

			return status;
		}

		private LoadStatus TryParseData(Stream data, PasswordInfo passwordInfo, Uri resource, Settings settings)
		{
			var dataFormat = dataFormats.FirstOrDefault(f => f.CanParse(data));
			var status = LoadStatus.NotSupported;

			if (dataFormat != null)
			{
				var result = dataFormat.TryParse(data, passwordInfo);
					
				logger.Info($"Tried to parse data from '{data}' using {dataFormat.GetType().Name} -> Result: {result.Status}.");

				if (result.Status == LoadStatus.Success || result.Status == LoadStatus.SuccessConfigureClient)
				{
					HandleParseSuccess(result, settings, passwordInfo, resource);
				}

				status = result.Status;
			}
			else
			{
				logger.Warn($"No data format found for '{data}'!");
			}

			return status;
		}

		private LoadStatus TryConfigureClient(IDictionary<string, object> data, Settings settings, PasswordInfo passwordInfo)
		{
			logger.Info("Attempting to configure local client settings...");

			if (passwordInfo.AdminPasswordHash != null)
			{
				var adminPasswordHash = passwordInfo.AdminPassword != null ? hashAlgorithm.GenerateHashFor(passwordInfo.AdminPassword) : null;
				var settingsPasswordHash = passwordInfo.SettingsPassword != null ? hashAlgorithm.GenerateHashFor(passwordInfo.SettingsPassword) : null;
				var enteredCorrectPassword = passwordInfo.AdminPasswordHash.Equals(adminPasswordHash, StringComparison.OrdinalIgnoreCase);
				var sameAdminPassword = passwordInfo.AdminPasswordHash.Equals(settings.AdminPasswordHash, StringComparison.OrdinalIgnoreCase);
				var knowsAdminPassword = passwordInfo.AdminPasswordHash.Equals(settingsPasswordHash, StringComparison.OrdinalIgnoreCase);

				if (sameAdminPassword || knowsAdminPassword || enteredCorrectPassword)
				{
					logger.Info("Authentication was successful.");
				}
				else
				{
					logger.Info("Authentication has failed!");

					return LoadStatus.AdminPasswordNeeded;
				}
			}
			else
			{
				logger.Info("Authentication is not required.");
			}

			// -> Certificates need to be imported and REMOVED from the settings before the data is saved!
			ExtractAndImportCertificates(data);

			// Save configuration data as local client config under %APPDATA%!
			// -> Default settings password for local client configuration appears to be string.Empty
			// -> Local client configuration needs to again be encrypted in the same way as the original file was!!

			return LoadStatus.SuccessConfigureClient;
		}

		private void UpdateAppConfig()
		{
			appConfig.ClientId = Guid.NewGuid();
			appConfig.ClientAddress = $"{BASE_ADDRESS}/client/{Guid.NewGuid()}";
		}
	}
}
