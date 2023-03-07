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
	public class FeatureConfigurationMonitor : IFeatureConfigurationMonitor
	{
		private readonly object @lock = new object();

		private IDictionary<IFeatureConfiguration, FeatureConfigurationStatus> configurations;
		private ILogger logger;
		private readonly int timeout_ms;
		private bool running;

		public FeatureConfigurationMonitor(ILogger logger, int timeout_ms)
		{
			if (timeout_ms < 0)
			{
				throw new ArgumentException("Must be 0 or greater!", nameof(timeout_ms));
			}

			this.configurations = new Dictionary<IFeatureConfiguration, FeatureConfigurationStatus>();
			this.logger = logger;
			this.timeout_ms = timeout_ms;
		}

		public void Observe(IFeatureConfiguration configuration, FeatureConfigurationStatus status)
		{
			lock (@lock)
			{
				configurations.Add(configuration, status);
			}
		}

		public void Reset()
		{
			lock (@lock)
			{
				if (running)
				{
					running = false;
					configurations.Clear();
					logger.Info("Stopped monitoring.");
				}
				else
				{
					logger.Info("Monitoring is not running.");
				}
			}
		}

		public void Start()
		{
			lock (@lock)
			{
				if (!running)
				{
					running = true;
					Task.Run(new Action(Monitor));
					logger.Info("Started monitoring.");
				}
				else
				{
					logger.Info("Monitoring is already running.");
				}
			}
		}

		private void Monitor()
		{
			if (IsStopped())
			{
				return;
			}

			var configurations = new Dictionary<IFeatureConfiguration, FeatureConfigurationStatus>(this.configurations);

			logger.Debug($"Checking {configurations.Count} configurations...");

			foreach (var item in configurations)
			{
				var configuration = item.Key;
				var status = item.Value;

				Enforce(configuration, status);

				if (IsStopped())
				{
					logger.Info("Monitoring was aborted.");

					return;
				}
			}

			if (configurations.Any())
			{
				Task.Delay(timeout_ms).ContinueWith((_) => Monitor());
			}
			else
			{
				lock (@lock)
				{
					running = false;
					logger.Info("Nothing to be monitored, stopped monitoring.");
				}
			}
		}

		private void Enforce(IFeatureConfiguration configuration, FeatureConfigurationStatus status)
		{
			var currentStatus = configuration.GetStatus();

			if (currentStatus != status)
			{
				logger.Warn($"{configuration} is {currentStatus.ToString().ToLower()} instead of {status.ToString().ToLower()}!");

				if (status == FeatureConfigurationStatus.Disabled)
				{
					configuration.DisableFeature();
				}
				else if (status == FeatureConfigurationStatus.Enabled)
				{
					configuration.EnableFeature();
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
	}
}
