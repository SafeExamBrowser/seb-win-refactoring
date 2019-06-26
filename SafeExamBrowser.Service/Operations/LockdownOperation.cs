/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Lockdown;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Service.Operations
{
	internal class LockdownOperation : SessionOperation
	{
		private IFeatureConfigurationBackup backup;
		private IFeatureConfigurationFactory factory;
		private ILogger logger;
		private Guid groupId;

		public LockdownOperation(
			IFeatureConfigurationBackup backup,
			IFeatureConfigurationFactory factory,
			ILogger logger,
			SessionContext sessionContext) : base(sessionContext)
		{
			this.backup = backup;
			this.factory = factory;
			this.logger = logger;
		}

		public override OperationResult Perform()
		{
			groupId = Guid.NewGuid();
			logger.Info($"Attempting to perform lockdown (feature configuration group: {groupId})...");

			var chromeNotification = factory.CreateChromeNotificationConfiguration(groupId);
			var easeOfAccess = factory.CreateEaseOfAccessConfiguration(groupId);
			var networkOptions = factory.CreateNetworkOptionsConfiguration(groupId);
			var passwordChange = factory.CreatePasswordChangeConfiguration(groupId);
			var powerOptions = factory.CreatePowerOptionsConfiguration(groupId);
			var remoteConnection = factory.CreateRemoteConnectionConfiguration(groupId);
			var signout = factory.CreateSignoutConfiguration(groupId);
			var taskManager = factory.CreateTaskManagerConfiguration(groupId);
			var userLock = factory.CreateUserLockConfiguration(groupId);
			var userSwitch = factory.CreateUserSwitchConfiguration(groupId);
			var vmwareOverlay = factory.CreateVmwareOverlayConfiguration(groupId);
			var windowsUpdate = factory.CreateWindowsUpdateConfiguration(groupId);

			SetConfiguration(chromeNotification, Context.Configuration.Settings.Service.DisableChromeNotifications);
			SetConfiguration(easeOfAccess, Context.Configuration.Settings.Service.DisableEaseOfAccessOptions);
			SetConfiguration(networkOptions, Context.Configuration.Settings.Service.DisableNetworkOptions);
			SetConfiguration(passwordChange, Context.Configuration.Settings.Service.DisablePasswordChange);
			SetConfiguration(powerOptions, Context.Configuration.Settings.Service.DisablePowerOptions);
			SetConfiguration(remoteConnection, Context.Configuration.Settings.Service.DisableRemoteConnections);
			SetConfiguration(signout, Context.Configuration.Settings.Service.DisableSignout);
			SetConfiguration(taskManager, Context.Configuration.Settings.Service.DisableTaskManager);
			SetConfiguration(userLock, Context.Configuration.Settings.Service.DisableUserLock);
			SetConfiguration(userSwitch, Context.Configuration.Settings.Service.DisableUserSwitch);
			SetConfiguration(vmwareOverlay, Context.Configuration.Settings.Service.DisableVmwareOverlay);
			SetConfiguration(windowsUpdate, Context.Configuration.Settings.Service.DisableWindowsUpdate);

			logger.Info("Lockdown successful.");

			return OperationResult.Success;
		}

		public override OperationResult Revert()
		{
			logger.Info($"Attempting to revert lockdown (feature configuration group: {groupId})...");

			var configurations = backup.GetBy(groupId);

			foreach (var configuration in configurations)
			{
				configuration.Restore();
				backup.Delete(configuration);
			}

			logger.Info("Lockdown reversion successful.");

			return OperationResult.Success;
		}

		private void SetConfiguration(IFeatureConfiguration configuration, bool disable)
		{
			backup.Save(configuration);

			if (disable)
			{
				configuration.DisableFeature();
			}
			else
			{
				configuration.EnableFeature();
			}

			configuration.Monitor();
		}
	}
}
