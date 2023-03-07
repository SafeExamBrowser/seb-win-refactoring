/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SafeExamBrowser.Configuration.ConfigurationData;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Configuration.Contracts.DataFormats;
using SafeExamBrowser.Configuration.Contracts.DataResources;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Settings;

namespace SafeExamBrowser.Configuration
{
	public class ConfigurationRepository : IConfigurationRepository
	{
		private readonly ICertificateStore certificateStore;
		private readonly IList<IDataParser> dataParsers;
		private readonly IList<IDataSerializer> dataSerializers;
		private readonly DataMapper dataMapper;
		private readonly DataProcessor dataProcessor;
		private readonly DataValues dataValues;
		private readonly ILogger logger;
		private readonly IList<IResourceLoader> resourceLoaders;
		private readonly IList<IResourceSaver> resourceSavers;

		public ConfigurationRepository(ICertificateStore certificateStore, IModuleLogger logger)
		{
			this.certificateStore = certificateStore;
			this.logger = logger;

			dataParsers = new List<IDataParser>();
			dataSerializers = new List<IDataSerializer>();
			dataMapper = new DataMapper();
			dataProcessor = new DataProcessor();
			dataValues = new DataValues();
			resourceLoaders = new List<IResourceLoader>();
			resourceSavers = new List<IResourceSaver>();
		}

		public SaveStatus ConfigureClientWith(Uri resource, PasswordParameters password = null)
		{
			logger.Info($"Attempting to configure local client with '{resource}'...");

			try
			{
				TryLoadData(resource, out var data);

				using (data)
				{
					TryParseData(data, out var encryption, out var format, out var rawData, password);

					certificateStore.ExtractAndImportIdentities(rawData);
					encryption = DetermineEncryptionForClientConfiguration(rawData, encryption);

					var status = TrySerializeData(rawData, format, out var serialized, encryption);

					using (serialized)
					{
						if (status == SaveStatus.Success)
						{
							status = TrySaveData(new Uri(dataValues.GetAppDataFilePath()), serialized);
						}

						return status;
					}
				}
			}
			catch (Exception e)
			{
				logger.Error($"Unexpected error while trying to configure local client with '{resource}'!", e);

				return SaveStatus.UnexpectedError;
			}
		}

		public AppConfig InitializeAppConfig()
		{
			return dataValues.InitializeAppConfig();
		}

		public SessionConfiguration InitializeSessionConfiguration()
		{
			return dataValues.InitializeSessionConfiguration();
		}

		public AppSettings LoadDefaultSettings()
		{
			return dataValues.LoadDefaultSettings();
		}

		public void Register(IDataParser parser)
		{
			dataParsers.Add(parser);
		}

		public void Register(IDataSerializer serializer)
		{
			dataSerializers.Add(serializer);
		}

		public void Register(IResourceLoader loader)
		{
			resourceLoaders.Add(loader);
		}

		public void Register(IResourceSaver saver)
		{
			resourceSavers.Add(saver);
		}

		public LoadStatus TryLoadSettings(Uri resource, out AppSettings settings, PasswordParameters password = null)
		{
			logger.Info($"Attempting to load '{resource}'...");

			settings = LoadDefaultSettings();

			try
			{
				var status = TryLoadData(resource, out var stream);

				using (stream)
				{
					if (status == LoadStatus.Success)
					{
						status = TryParseData(stream, out _, out _, out var data, password);

						if (status == LoadStatus.Success)
						{
							dataMapper.MapRawDataToSettings(data, settings);
							dataProcessor.Process(data, settings);
						}
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

		private EncryptionParameters DetermineEncryptionForClientConfiguration(IDictionary<string, object> data, EncryptionParameters encryption)
		{
			var hasKey = data.TryGetValue(Keys.ConfigurationFile.KeepClientConfigEncryption, out var value);
			var useDefaultEncryption = value is bool keepEncryption && !keepEncryption;

			if (!hasKey || (hasKey && useDefaultEncryption))
			{
				encryption = new PasswordParameters { Password = string.Empty, IsHash = true };
			}

			return encryption;
		}

		private LoadStatus TryLoadData(Uri resource, out Stream data)
		{
			var status = LoadStatus.NotSupported;
			var resourceLoader = resourceLoaders.FirstOrDefault(l => l.CanLoad(resource));

			data = default;

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

		private LoadStatus TryParseData(Stream data, out EncryptionParameters encryption, out FormatType format, out IDictionary<string, object> rawData, PasswordParameters password = null)
		{
			var parser = dataParsers.FirstOrDefault(p => p.CanParse(data));
			var status = LoadStatus.NotSupported;

			encryption = default;
			format = default;
			rawData = default(Dictionary<string, object>);

			if (parser != null)
			{
				var result = parser.TryParse(data, password);

				encryption = result.Encryption;
				format = result.Format;
				rawData = result.RawData;
				status = result.Status;

				logger.Info($"Tried to parse data from '{data}' using {parser.GetType().Name} -> Result: {status}.");
			}
			else
			{
				logger.Warn($"No data parser found which can parse '{data}'!");
			}

			return status;
		}

		private SaveStatus TrySaveData(Uri destination, Stream data)
		{
			var status = SaveStatus.NotSupported;
			var resourceSaver = resourceSavers.FirstOrDefault(s => s.CanSave(destination));

			if (resourceSaver != null)
			{
				status = resourceSaver.TrySave(destination, data);
				logger.Info($"Tried to save data as '{destination}' using {resourceSaver.GetType().Name} -> Result: {status}.");
			}
			else
			{
				logger.Warn($"No resource saver found for '{destination}'!");
			}

			return status;
		}

		private SaveStatus TrySerializeData(IDictionary<string, object> data, FormatType format, out Stream serialized, EncryptionParameters encryption = null)
		{
			var serializer = dataSerializers.FirstOrDefault(s => s.CanSerialize(format));
			var status = SaveStatus.NotSupported;

			serialized = default;

			if (serializer != null)
			{
				var result = serializer.TrySerialize(data, encryption);

				serialized = result.Data;
				status = result.Status;

				logger.Info($"Tried to serialize data as '{format}' using {serializer.GetType().Name} -> Result: {status}.");
			}
			else
			{
				logger.Error($"No data serializer found which can serialize '{format}'!");
			}

			return status;
		}
	}
}
