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
using System.IO.Compression;
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
		private IList<IDataFormat> dataFormats;
		private ILogger logger;
		private IList<IResourceLoader> resourceLoaders;

		public ConfigurationRepository(ILogger logger, string executablePath, string programCopyright, string programTitle, string programVersion)
		{
			dataFormats = new List<IDataFormat>();
			resourceLoaders = new List<IResourceLoader>();

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

		public LoadStatus TryLoadSettings(Uri resource, out Settings settings, string adminPassword = null, string settingsPassword = null)
		{
			settings = default(Settings);

			logger.Info($"Attempting to load '{resource}'...");

			try
			{
				var resourceLoader = resourceLoaders.FirstOrDefault(l => l.CanLoad(resource));

				if (resourceLoader != null)
				{
					var data = resourceLoader.Load(resource);
					var dataFormat = dataFormats.FirstOrDefault(f => f.CanParse(data));

					logger.Info($"Successfully loaded {data.Length / 1000.0} KB data from '{resource}' using {resourceLoader.GetType().Name}.");

					if (dataFormat is HtmlFormat)
					{
						return HandleHtml(resource, out settings);
					}

					if (dataFormat != null)
					{
						return dataFormat.TryParse(data, out settings, adminPassword, settingsPassword);
					}
				}

				logger.Warn($"No {(resourceLoader == null ? "resource loader" : "data format")} found for '{resource}'!");

				return LoadStatus.NotSupported;
			}
			catch (Exception e)
			{
				logger.Error($"Unexpected error while trying to load '{resource}'!", e);

				return LoadStatus.UnexpectedError;
			}
		}

		private LoadStatus HandleHtml(Uri resource, out Settings settings)
		{
			logger.Info($"Loaded data appears to be HTML, loading default settings and using '{resource}' as startup URL.");

			settings = LoadDefaultSettings();
			settings.Browser.StartUrl = resource.AbsoluteUri;

			return LoadStatus.Success;
		}

		private byte[] Decompress(byte[] bytes)
		{
			try
			{
				var buffer = new byte[4096];

				using (var stream = new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress))
				using (var decompressed = new MemoryStream())
				{
					var bytesRead = 0;

					do
					{
						bytesRead = stream.Read(buffer, 0, buffer.Length);
						decompressed.Write(buffer, 0, bytesRead);
					} while (bytesRead > 0);

					return decompressed.ToArray();
				}
			}
			catch (InvalidDataException)
			{
				return bytes;
			}
		}

		private void UpdateAppConfig()
		{
			appConfig.ClientId = Guid.NewGuid();
			appConfig.ClientAddress = $"{BASE_ADDRESS}/client/{Guid.NewGuid()}";
		}
	}
}
