/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using SafeExamBrowser.Lockdown.Contracts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Lockdown.FeatureConfigurations.RegistryConfigurations
{
	[Serializable]
	internal abstract class RegistryConfiguration : FeatureConfiguration
	{
		private IList<RegistryDataItem> itemsToDelete;
		private IList<RegistryDataItem> itemsToRestore;

		protected abstract IEnumerable<RegistryConfigurationItem> Items { get; }
		protected abstract RegistryKey RootKey { get; }

		public RegistryConfiguration(Guid groupId, ILogger logger) : base(groupId, logger)
		{
			itemsToDelete = new List<RegistryDataItem>();
			itemsToRestore = new List<RegistryDataItem>();
		}

		protected abstract bool IsHiveAvailable(RegistryDataItem item);

		public override bool DisableFeature()
		{
			var success = true;

			foreach (var item in Items)
			{
				success &= TrySet(new RegistryDataItem { Key = item.Key, Value = item.Value, Data = item.Disabled });
			}

			if (success)
			{
				logger.Info("Successfully disabled feature.");
			}
			else
			{
				logger.Warn("Failed to disable feature!");
			}

			return success;
		}

		public override bool EnableFeature()
		{
			var success = true;

			foreach (var item in Items)
			{
				success &= TrySet(new RegistryDataItem { Key = item.Key, Value = item.Value, Data = item.Enabled });
			}

			if (success)
			{
				logger.Info("Successfully enabled feature.");
			}
			else
			{
				logger.Warn("Failed to enable feature!");
			}

			return success;
		}

		public override void Initialize()
		{
			foreach (var item in Items)
			{
				var original = ReadItem(item.Key, item.Value);

				if (original.Data == null)
				{
					itemsToDelete.Add(original);
				}
				else
				{
					itemsToRestore.Add(original);
				}
			}
		}

		public override FeatureConfigurationStatus GetStatus()
		{
			var status = FeatureConfigurationStatus.Undefined;

			foreach (var item in Items)
			{
				var current = ReadItem(item.Key, item.Value);

				if (current.Data?.Equals(item.Disabled) == true && status != FeatureConfigurationStatus.Enabled)
				{
					status = FeatureConfigurationStatus.Disabled;
				}
				else if (current.Data?.Equals(item.Enabled) == true && status != FeatureConfigurationStatus.Disabled)
				{
					status = FeatureConfigurationStatus.Enabled;
				}
				else
				{
					status = FeatureConfigurationStatus.Undefined;

					break;
				}
			}

			return status;
		}

		public override bool Restore()
		{
			foreach (var item in new List<RegistryDataItem>(itemsToDelete))
			{
				if (TryDelete(item))
				{
					itemsToDelete.Remove(item);
				}
			}

			foreach (var item in new List<RegistryDataItem>(itemsToRestore))
			{
				if (TrySet(item))
				{
					itemsToRestore.Remove(item);
				}
			}

			var success = !itemsToDelete.Any() && !itemsToRestore.Any();

			if (success)
			{
				logger.Info("Successfully restored feature.");
			}
			else
			{
				logger.Warn("Failed to restore feature!");
			}

			return success;
		}

		protected bool DeleteConfiguration()
		{
			var success = true;

			foreach (var item in Items)
			{
				success &= TryDelete(new RegistryDataItem { Key = item.Key, Value = item.Value });
			}

			if (success)
			{
				logger.Info("Successfully deleted feature configuration.");
			}
			else
			{
				logger.Warn("Failed to delete feature configuration!");
			}

			return success;
		}

		private RegistryDataItem ReadItem(string key, string value)
		{
			var data = Registry.GetValue(key, value, null);
			var original = new RegistryDataItem { Key = key, Value = value, Data = data };

			return original;
		}

		private bool TryDelete(RegistryDataItem item)
		{
			var success = false;

			try
			{
				if (IsHiveAvailable(item))
				{
					var keyWithoutRoot = item.Key.Substring(item.Key.IndexOf('\\') + 1);

					using (var key = RootKey.OpenSubKey(keyWithoutRoot, true))
					{
						if (key.GetValue(item.Value) != null)
						{
							key.DeleteValue(item.Value);
							logger.Debug($"Successfully deleted registry item {item}.");
						}
						else
						{
							logger.Debug($"No need to delete registry item {item} as it does not exist.");
						}

						success = true;
					}
				}
				else
				{
					logger.Warn($"Cannot delete item {item} as its registry hive is not available!");
				}
			}
			catch (Exception e)
			{
				logger.Error($"Failed to delete registry item {item}!", e);
			}

			return success;
		}

		private bool TrySet(RegistryDataItem item)
		{
			var success = false;

			try
			{
				if (IsHiveAvailable(item))
				{
					Registry.SetValue(item.Key, item.Value, item.Data);
					logger.Debug($"Successfully set registry item {item}.");
					success = true;
				}
				else
				{
					logger.Warn($"Cannot set item {item} as its registry hive is not available!");
				}
			}
			catch (Exception e)
			{
				logger.Error($"Failed to set registry item {item}!", e);
			}

			return success;
		}
	}
}
