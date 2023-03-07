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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using SafeExamBrowser.Lockdown.Contracts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Lockdown
{
	public class FeatureConfigurationBackup : IFeatureConfigurationBackup
	{
		private readonly object @lock = new object();

		private string filePath;
		private IModuleLogger logger;

		public FeatureConfigurationBackup(string filePath, IModuleLogger logger)
		{
			this.filePath = filePath;
			this.logger = logger;
		}

		public void Delete(IFeatureConfiguration configuration)
		{
			lock (@lock)
			{
				var configurations = LoadFromFile();
				var obsolete = configurations.Find(c => c.Id == configuration.Id);
				
				if (obsolete != default(IFeatureConfiguration))
				{
					configurations.Remove(obsolete);
					SaveToFile(configurations);
					logger.Info($"Successfully removed {configuration} from backup.");
				}
				else
				{
					logger.Warn($"Could not delete {configuration} as it does not exist in backup!");
				}
			}
		}

		public IList<IFeatureConfiguration> GetAllConfigurations()
		{
			lock (@lock)
			{
				return LoadFromFile();
			}
		}

		public IList<IFeatureConfiguration> GetBy(Guid groupId)
		{
			lock (@lock)
			{
				return LoadFromFile().Where(c => c.GroupId == groupId).ToList();
			}
		}

		public void Save(IFeatureConfiguration configuration)
		{
			lock (@lock)
			{
				var configurations = LoadFromFile();

				configurations.Add(configuration);
				SaveToFile(configurations);
				logger.Info($"Successfully added {configuration} to backup.");
			}
		}

		private List<IFeatureConfiguration> LoadFromFile()
		{
			var configurations = new List<IFeatureConfiguration>();

			try
			{
				if (File.Exists(filePath))
				{
					var context = new StreamingContext(StreamingContextStates.All, logger);

					logger.Debug($"Attempting to load backup data from '{filePath}'...");

					using (var stream = File.Open(filePath, FileMode.Open))
					{
						configurations = (List<IFeatureConfiguration>) new BinaryFormatter(null, context).Deserialize(stream);
					}

					logger.Debug($"Backup data successfully loaded, found {configurations.Count} items.");
				}
				else
				{
					logger.Debug($"No backup data found under '{filePath}'.");
				}
			}
			catch (Exception e)
			{
				logger.Error($"Failed to load backup data from '{filePath}'!", e);
			}

			return configurations;
		}

		private void SaveToFile(List<IFeatureConfiguration> configurations)
		{
			try
			{
				if (configurations.Any())
				{
					logger.Debug($"Attempting to save backup data to '{filePath}'...");

					using (var stream = File.Open(filePath, FileMode.Create))
					{
						new BinaryFormatter().Serialize(stream, configurations);
					}

					logger.Debug($"Successfully saved {configurations.Count} items.");
				}
				else
				{
					File.Delete(filePath);
					logger.Debug("No backup data to save, deleted backup file.");
				}
			}
			catch (Exception e)
			{
				logger.Error($"Failed to save backup data to '{filePath}'!", e);
			}
		}
	}
}
