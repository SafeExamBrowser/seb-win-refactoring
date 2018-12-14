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
using System.Security.Cryptography.X509Certificates;
using SafeExamBrowser.Configuration.DataFormats;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Cryptography;
using SafeExamBrowser.Contracts.Configuration.DataFormats;
using SafeExamBrowser.Contracts.Configuration.DataResources;
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
		private IList<IDataResource> dataResources;
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
			dataResources = new List<IDataResource>();

			this.hashAlgorithm = hashAlgorithm;
			this.logger = logger;
			this.executablePath = executablePath ?? string.Empty;
			this.programCopyright = programCopyright ?? string.Empty;
			this.programTitle = programTitle ?? string.Empty;
			this.programVersion = programVersion ?? string.Empty;
		}

		public SaveStatus ConfigureClientWith(Uri resource, PasswordParameters password = null)
		{
			logger.Info($"Attempting to configure local client settings from '{resource}'...");

			try
			{
				TryLoadData(resource, out var stream);

				using (stream)
				{
					TryParseData(stream, out var encryption, out var format, out var data, password);
					HandleIdentityCertificates(data);

					// TODO: Encrypt and save configuration data as local client config under %APPDATA%!
					// -> New key will determine whether to use default password or current settings password!
					//     -> "clientConfigEncryptUsingSettingsPassword"
					//     -> Default settings password for local client configuration appears to be string.Empty -> passwords.SettingsPassword
					//     -> Otherwise, the local client configuration must again be encrypted in the same way as the original file!!
				}

				logger.Info($"Successfully configured local client settings with '{resource}'.");

				return SaveStatus.Success;
			}
			catch (Exception e)
			{
				logger.Error($"Unexpected error while trying to configure local client settings '{resource}'!", e);

				return SaveStatus.UnexpectedError;
			}
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

		public void Register(IDataResource dataResource)
		{
			dataResources.Add(dataResource);
		}

		public LoadStatus TryLoadSettings(Uri resource, out Settings settings, PasswordParameters password = null)
		{
			logger.Info($"Attempting to load '{resource}'...");

			settings = LoadDefaultSettings();

			try
			{
				var status = TryLoadData(resource, out var stream);

				using (stream)
				{
					if (status != LoadStatus.Success)
					{
						return status;
					}

					status = TryParseData(stream, out _, out _, out var data, password);

					if (status == LoadStatus.Success)
					{
						data.MapTo(settings);
					}

					return status;
				}
			}
			catch (Exception e)
			{
				logger.Error($"Unexpected error while trying to load '{resource}'!", e);

				return LoadStatus.UnexpectedError;
			}
		}

		public SaveStatus TrySaveSettings(Uri resource, Format format, Settings settings, EncryptionParameters encryption = null)
		{
			throw new NotImplementedException();
		}

		private void HandleIdentityCertificates(IDictionary<string, object> data)
		{
			const int IDENTITY_CERTIFICATE = 1;
			var hasCertificates = data.TryGetValue("embeddedCertificates", out object value);

			if (hasCertificates && value is IList<IDictionary<string, object>> certificates)
			{
				var toRemove = new List<IDictionary<string, object>>();

				foreach (var certificate in certificates)
				{
					var isIdentity = certificate.TryGetValue("type", out var o) && o is int type && type == IDENTITY_CERTIFICATE;
					var hasData = certificate.TryGetValue("certificateData", out value);

					if (isIdentity && hasData && value is byte[] certificateData)
					{
						ImportIdentityCertificate(certificateData, new X509Store(StoreLocation.CurrentUser));
						ImportIdentityCertificate(certificateData, new X509Store(StoreName.TrustedPeople, StoreLocation.LocalMachine));

						toRemove.Add(certificate);
					}
				}

				toRemove.ForEach(c => certificates.Remove(c));
			}
		}

		private void ImportIdentityCertificate(byte[] certificateData, X509Store store)
		{
			try
			{
				var certificate = new X509Certificate2();

				certificate.Import(certificateData, "Di𝈭l𝈖Ch𝈒ah𝉇t𝈁a𝉈Hai1972", X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.PersistKeySet);

				store.Open(OpenFlags.ReadWrite);
				store.Add(certificate);

				logger.Info($"Successfully imported identity certificate into {store.Location}.{store.Name}.");
			}
			catch (Exception e)
			{
				logger.Error($"Failed to import identity certificate into {store.Location}.{store.Name}!", e);
			}
			finally
			{
				store.Close();
			}
		}

		private LoadStatus TryLoadData(Uri resource, out Stream data)
		{
			var status = LoadStatus.NotSupported;
			var resourceLoader = dataResources.FirstOrDefault(l => l.CanLoad(resource));

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

		private LoadStatus TryParseData(Stream data, out EncryptionParameters encryption, out Format format, out IDictionary<string, object> rawData, PasswordParameters password = null)
		{
			var dataFormat = dataFormats.FirstOrDefault(f => f.CanParse(data));
			var status = LoadStatus.NotSupported;

			encryption = default(EncryptionParameters);
			format = default(Format);
			rawData = default(Dictionary<string, object>);

			if (dataFormat != null)
			{
				var result = dataFormat.TryParse(data, password);

				encryption = result.Encryption;
				format = result.Format;
				rawData = result.RawData;
				status = result.Status;

				logger.Info($"Tried to parse data from '{data}' using {dataFormat.GetType().Name} -> Result: {status}.");
			}
			else
			{
				logger.Warn($"No data format found for '{data}'!");
			}

			return status;
		}

		private void UpdateAppConfig()
		{
			appConfig.ClientId = Guid.NewGuid();
			appConfig.ClientAddress = $"{BASE_ADDRESS}/client/{Guid.NewGuid()}";
		}
	}
}
