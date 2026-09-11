/*
 * Copyright (c) 2026 ETH Zürich, IT Services
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
	/// <summary>
	/// Security fix (CWE-502): Restricts BinaryFormatter deserialization to only known, safe types.
	/// Without this binder, an attacker who can replace the backup file could craft a malicious
	/// payload that executes arbitrary code during deserialization (e.g. via gadget chains).
	/// </summary>
	internal sealed class SafeConfigurationBinder : SerializationBinder
	{
		private static readonly HashSet<string> AllowedTypeNames = new HashSet<string>(StringComparer.Ordinal)
		{
			// Only allow the base configuration types that are legitimately serialized
			"SafeExamBrowser.Lockdown.FeatureConfigurations.FeatureConfiguration",
			"SafeExamBrowser.Lockdown.FeatureConfigurations.RegistryConfiguration",
			"SafeExamBrowser.Lockdown.FeatureConfigurations.ServiceConfiguration",
			// Allow lists and dictionaries used by the configuration types
			"System.Collections.Generic.List`1",
			"System.Collections.Generic.Dictionary`2",
			// Allow primitive types that appear in configuration data
			"System.String",
			"System.Int32",
			"System.Boolean",
			"System.Guid"
		};

		public override Type BindToType(string assemblyName, string typeName)
		{
			if (AllowedTypeNames.Contains(typeName))
			{
				return Type.GetType($"{typeName}, {assemblyName}");
			}

			// Reject any type not on the whitelist
			return null;
		}
	}
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

					// Security fix (CWE-502): Use a SerializationBinder to restrict deserialization
					// to only known, safe types. Without this, BinaryFormatter can instantiate
					// arbitrary types, allowing code execution if the backup file is tampered with.
					var formatter = new BinaryFormatter(null, context)
					{
						Binder = new SafeConfigurationBinder()
					};

					using (var stream = File.Open(filePath, FileMode.Open))
					{
						configurations = (List<IFeatureConfiguration>) formatter.Deserialize(stream);
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
