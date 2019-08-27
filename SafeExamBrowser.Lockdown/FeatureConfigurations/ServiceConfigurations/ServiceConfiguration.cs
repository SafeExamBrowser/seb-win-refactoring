/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using SafeExamBrowser.Contracts.Lockdown;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Lockdown.FeatureConfigurations.ServiceConfigurations
{
	[Serializable]
	internal abstract class ServiceConfiguration : FeatureConfiguration
	{
		private static readonly TimeSpan TEN_SECONDS = TimeSpan.FromSeconds(10);
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
				success &= TrySet(new ServiceDataItem { Name = item.Name, Status = item.Disabled });
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
				success &= TrySet(new ServiceDataItem { Name = item.Name, Status = item.Enabled });
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
				if (TrySet(item))
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

		private bool TrySet(ServiceDataItem item)
		{
			var success = false;

			try
			{
				if (IsAvailable(item))
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
				else
				{
					logger.Warn($"Cannot set service {item} as it does not exist!");
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
			var success = true;

			if (service.Status == ServiceControllerStatus.PausePending)
			{
				service.WaitForStatus(ServiceControllerStatus.Paused, TEN_SECONDS);
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
				service.WaitForStatus(ServiceControllerStatus.Running, TEN_SECONDS);
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
				service.WaitForStatus(ServiceControllerStatus.Running, TEN_SECONDS);
				service.Refresh();
			}

			if (service.Status == ServiceControllerStatus.Running)
			{
				service.Stop();
				service.Refresh();
			}

			if (service.Status == ServiceControllerStatus.StopPending)
			{
				service.WaitForStatus(ServiceControllerStatus.Stopped, TEN_SECONDS);
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
			return ServiceController.GetServices().Any(s => s.ServiceName == item.Name);
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
