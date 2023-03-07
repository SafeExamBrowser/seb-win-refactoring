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
using System.ServiceProcess;
using SafeExamBrowser.Lockdown.Contracts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Lockdown.FeatureConfigurations.ServiceConfigurations
{
	[Serializable]
	internal abstract class ServiceConfiguration : FeatureConfiguration
	{
		private static readonly TimeSpan FIVE_SECONDS = TimeSpan.FromSeconds(5);
		private const int MAX_ATTEMPTS = 5;

		private IList<ServiceDataItem> originalItems;

		protected abstract IEnumerable<ServiceConfigurationItem> Items { get; }

		public ServiceConfiguration(Guid groupId, ILogger logger) : base(groupId, logger)
		{
			originalItems = new List<ServiceDataItem>();
		}

		public override bool DisableFeature()
		{
			var success = true;

			foreach (var item in Items)
			{
				success &= TrySetWithRetry(new ServiceDataItem { Name = item.Name, Status = item.Disabled });
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
				success &= TrySetWithRetry(new ServiceDataItem { Name = item.Name, Status = item.Enabled });
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

		public override FeatureConfigurationStatus GetStatus()
		{
			var status = FeatureConfigurationStatus.Undefined;

			foreach (var item in Items)
			{
				var current = ReadService(item.Name);

				if (current.Status == item.Disabled && status != FeatureConfigurationStatus.Enabled)
				{
					status = FeatureConfigurationStatus.Disabled;
				}
				else if (current.Status == item.Enabled && status != FeatureConfigurationStatus.Disabled)
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

		public override void Initialize()
		{
			foreach (var item in Items)
			{
				var original = ReadService(item.Name);

				if (original.Status != ServiceStatus.NotAvailable)
				{
					originalItems.Add(original);
				}
			}
		}

		public override bool Restore()
		{
			foreach (var item in new List<ServiceDataItem>(originalItems))
			{
				if (TrySetWithRetry(item))
				{
					originalItems.Remove(item);
				}
			}

			var success = !originalItems.Any();

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

		private ServiceDataItem ReadService(string name)
		{
			var item = new ServiceDataItem { Name = name, Status = ServiceStatus.NotAvailable };

			try
			{
				using (var service = new ServiceController(name))
				{
					item.Status = ToStatus(service.Status);
				}
			}
			catch (Exception e)
			{
				logger.Error($"Failed to retrieve status of service '{name}'!", e);
			}

			return item;
		}

		private bool TrySetWithRetry(ServiceDataItem item)
		{
			var success = false;

			if (IsAvailable(item))
			{
				for (var attempt = 1; attempt <= MAX_ATTEMPTS && !success; attempt++)
				{
					logger.Debug($"Attempt {attempt}/{MAX_ATTEMPTS} to set service {item}...");
					success = TrySet(item);
				}

				if (!success)
				{
					logger.Error($"All attempts to set service {item} have failed!");
				}
			}
			else
			{
				logger.Warn($"Cannot set service {item} as it does not exist!");
			}

			return success;
		}

		private bool TrySet(ServiceDataItem item)
		{
			var success = false;

			try
			{
				using (var service = new ServiceController(item.Name))
				{
					if (item.Status == ServiceStatus.Running)
					{
						success = TryStart(service);
					}
					else if (item.Status == ServiceStatus.Stopped)
					{
						success = TryStop(service);
					}

					if (success)
					{
						logger.Debug($"Successfully set service {item}.");
					}
					else
					{
						logger.Warn($"Could not set service {item}! Current status: {service.Status}.");
					}
				}
			}
			catch (Exception e)
			{
				logger.Error($"Failed to set service {item}!", e);
			}

			return success;
		}

		private bool TryStart(ServiceController service)
		{
			var success = false;

			if (service.Status == ServiceControllerStatus.PausePending)
			{
				service.WaitForStatus(ServiceControllerStatus.Paused, FIVE_SECONDS);
				service.Refresh();
			}

			if (service.Status == ServiceControllerStatus.Paused)
			{
				service.Continue();
				service.Refresh();
			}

			if (service.Status == ServiceControllerStatus.Stopped)
			{
				service.Start();
				service.Refresh();
			}

			if (service.Status == ServiceControllerStatus.StartPending || service.Status == ServiceControllerStatus.ContinuePending)
			{
				service.WaitForStatus(ServiceControllerStatus.Running, FIVE_SECONDS);
				service.Refresh();
			}

			if (service.Status == ServiceControllerStatus.Running)
			{
				success = true;
			}

			return success;
		}

		private bool TryStop(ServiceController service)
		{
			var success = false;

			if (service.Status == ServiceControllerStatus.Paused)
			{
				service.Continue();
				service.Refresh();
			}

			if (service.Status == ServiceControllerStatus.StartPending || service.Status == ServiceControllerStatus.ContinuePending)
			{
				service.WaitForStatus(ServiceControllerStatus.Running, FIVE_SECONDS);
				service.Refresh();
			}

			if (service.Status == ServiceControllerStatus.Running)
			{
				service.Stop();
				service.Refresh();
			}

			if (service.Status == ServiceControllerStatus.StopPending)
			{
				service.WaitForStatus(ServiceControllerStatus.Stopped, FIVE_SECONDS);
				service.Refresh();
			}

			if (service.Status == ServiceControllerStatus.Stopped)
			{
				success = true;
			}

			return success;
		}

		private bool IsAvailable(ServiceDataItem item)
		{
			var available = false;

			try
			{
				available = ServiceController.GetServices().Any(s => s.ServiceName == item.Name);
			}
			catch (Exception e)
			{
				logger.Error($"Failed to check whether service '{item.Name}' is available!", e);
			}

			return available;
		}

		private ServiceStatus ToStatus(ServiceControllerStatus status)
		{
			switch (status)
			{
				case ServiceControllerStatus.ContinuePending:
				case ServiceControllerStatus.Running:
				case ServiceControllerStatus.StartPending:
					return ServiceStatus.Running;
				default:
					return ServiceStatus.Stopped;
			}
		}
	}
}
