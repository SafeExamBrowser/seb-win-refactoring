/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Lockdown.Contracts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Service.Operations
{
	internal class LockdownOperation : SessionOperation
	{
		private readonly IFeatureConfigurationBackup backup;
		private readonly IFeatureConfigurationFactory factory;
		private readonly IFeatureConfigurationMonitor monitor;
		private readonly ILogger logger;

		private Guid groupId;

		public LockdownOperation(
			IFeatureConfigurationBackup backup,
			IFeatureConfigurationFactory factory,
			IFeatureConfigurationMonitor monitor,
			ILogger logger,
			SessionContext sessionContext) : base(sessionContext)
		{
			this.backup = backup;
			this.factory = factory;
			this.monitor = monitor;
			this.logger = logger;
		}

		public override OperationResult Perform()
		{
			groupId = Guid.NewGuid();

			var success = true;
			var sid = Context.Configuration.UserSid;
			var userName = Context.Configuration.UserName;
			var configurations = new List<(IFeatureConfiguration, bool)>
			{
				(factory.CreateChangePasswordConfiguration(groupId, sid, userName), Context.Configuration.Settings.Service.DisablePasswordChange),
				(factory.CreateChromeNotificationConfiguration(groupId, sid, userName), Context.Configuration.Settings.Service.DisableChromeNotifications),
				(factory.CreateEaseOfAccessConfiguration(groupId), Context.Configuration.Settings.Service.DisableEaseOfAccessOptions),
				(factory.CreateFindPrinterConfiguration(groupId, sid, userName), Context.Configuration.Settings.Service.DisableFindPrinter),
				(factory.CreateLockWorkstationConfiguration(groupId, sid, userName), Context.Configuration.Settings.Service.DisableUserLock),
				(factory.CreateMachinePowerOptionsConfiguration(groupId), Context.Configuration.Settings.Service.DisablePowerOptions),
				(factory.CreateNetworkOptionsConfiguration(groupId), Context.Configuration.Settings.Service.DisableNetworkOptions),
				(factory.CreateRemoteConnectionConfiguration(groupId), Context.Configuration.Settings.Service.DisableRemoteConnections),
				(factory.CreateSignoutConfiguration(groupId, sid, userName), Context.Configuration.Settings.Service.DisableSignout),
				(factory.CreateSwitchUserConfiguration(groupId), Context.Configuration.Settings.Service.DisableUserSwitch),
				(factory.CreateTaskManagerConfiguration(groupId, sid, userName), Context.Configuration.Settings.Service.DisableTaskManager),
				(factory.CreateUserPowerOptionsConfiguration(groupId, sid, userName), Context.Configuration.Settings.Service.DisablePowerOptions),
				(factory.CreateWindowsUpdateConfiguration(groupId), Context.Configuration.Settings.Service.DisableWindowsUpdate)
			};

			if (Context.Configuration.Settings.Service.SetVmwareConfiguration)
			{
				configurations.Add((factory.CreateVmwareOverlayConfiguration(groupId, sid, userName), Context.Configuration.Settings.Service.DisableVmwareOverlay));
			}

			logger.Info($"Attempting to perform lockdown (feature configuration group: {groupId})...");

			foreach (var (configuration, disable) in configurations)
			{
				success &= TrySet(configuration, disable);

				if (!success)
				{
					break;
				}
			}

			if (success)
			{
				monitor.Start();
				logger.Info("Lockdown successful.");
			}
			else
			{
				logger.Error("Lockdown was not successful!");
			}

			return success ? OperationResult.Success : OperationResult.Failed;
		}

		public override OperationResult Revert()
		{
			logger.Info($"Attempting to revert lockdown (feature configuration group: {groupId})...");

			var configurations = backup.GetBy(groupId);
			var success = true;

			monitor.Reset();

			foreach (var configuration in configurations)
			{
				success &= TryRestore(configuration);
			}

			if (success)
			{
				logger.Info("Lockdown reversion successful.");
			}
			else
			{
				logger.Warn("Lockdown reversion was not successful!");
			}

			return success ? OperationResult.Success : OperationResult.Failed;
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
				logger.Error($"Failed to restore {configuration}!");
			}

			return success;
		}

		private bool TrySet(IFeatureConfiguration configuration, bool disable)
		{
			var success = false;
			var status = FeatureConfigurationStatus.Undefined;

			configuration.Initialize();
			backup.Save(configuration);

			if (disable)
			{
				success = configuration.DisableFeature();
				status = FeatureConfigurationStatus.Disabled;
			}
			else
			{
				success = configuration.EnableFeature();
				status = FeatureConfigurationStatus.Enabled;
			}

			if (success)
			{
				monitor.Observe(configuration, status);
			}
			else
			{
				logger.Error($"Failed to configure {configuration}!");
			}

			return success;
		}
	}
}
