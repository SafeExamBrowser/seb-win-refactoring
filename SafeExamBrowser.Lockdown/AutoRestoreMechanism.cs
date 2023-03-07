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
using System.Threading.Tasks;
using SafeExamBrowser.Lockdown.Contracts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Lockdown
{
	public class AutoRestoreMechanism : IAutoRestoreMechanism
	{
		private readonly object @lock = new object();

		private IFeatureConfigurationBackup backup;
		private ILogger logger;
		private ISystemConfigurationUpdate systemConfigurationUpdate;
		private bool running;
		private int timeout_ms;

		public AutoRestoreMechanism(
			IFeatureConfigurationBackup backup,
			ILogger logger,
			ISystemConfigurationUpdate systemConfigurationUpdate,
			int timeout_ms)
		{
			if (timeout_ms < 0)
			{
				throw new ArgumentException("Must be 0 or greater!", nameof(timeout_ms));
			}

			this.backup = backup;
			this.logger = logger;
			this.systemConfigurationUpdate = systemConfigurationUpdate;
			this.timeout_ms = timeout_ms;
		}

		public void Start()
		{
			lock (@lock)
			{
				if (!running)
				{
					running = true;
					Task.Run(new Action(AutoRestore));
					logger.Info("Started auto-restore mechanism.");
				}
				else
				{
					logger.Info("Auto-restore mechanism is already running.");
				}
			}
		}

		public void Stop()
		{
			lock (@lock)
			{
				if (running)
				{
					running = false;
					logger.Info("Stopped auto-restore mechanism.");
				}
				else
				{
					logger.Info("Auto-restore mechanism is not running.");
				}
			}
		}

		private void AutoRestore()
		{
			if (IsStopped())
			{
				return;
			}

			var configurations = backup.GetAllConfigurations();

			if (configurations.Any())
			{
				var success = TryRestoreAll(configurations);

				if (success == true)
				{
					systemConfigurationUpdate.ExecuteAsync();
				}
				else if (success == false)
				{
					Task.Delay(timeout_ms).ContinueWith((_) => AutoRestore());
				}
				else
				{
					logger.Info("Auto-restore mechanism was aborted.");
				}
			}
			else
			{
				lock (@lock)
				{
					running = false;
					logger.Info("Nothing to restore, stopped auto-restore mechanism.");
				}
			}
		}

		private bool IsStopped()
		{
			lock (@lock)
			{
				return !running;
			}
		}

		private bool? TryRestoreAll(IList<IFeatureConfiguration> configurations)
		{
			var restored = 0;

			logger.Info($"Attempting to restore {configurations.Count} items...");

			foreach (var configuration in configurations)
			{
				var success = TryRestore(configuration);

				if (success)
				{
					restored++;
				}

				if (IsStopped())
				{
					return null;
				}
			}

			logger.Info($"Restored {restored} of {configurations.Count} items.");

			return restored == configurations.Count;
		}

		private bool TryRestore(IFeatureConfiguration configuration)
		{
			var success = configuration.Restore();

			if (success)
			{
				backup.Delete(configuration);
			}
			else
			{
				logger.Warn($"Failed to restore {configuration}!");
			}

			return success;
		}
	}
}
