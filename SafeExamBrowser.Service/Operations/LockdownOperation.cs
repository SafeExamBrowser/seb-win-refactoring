/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

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
			var chromeNotification = factory.CreateChromeNotificationConfiguration();
			var easeOfAccess = factory.CreateEaseOfAccessConfiguration();
			var networkOptions = factory.CreateNetworkOptionsConfiguration();
			var passwordChange = factory.CreatePasswordChangeConfiguration();
			var powerOptions = factory.CreatePowerOptionsConfiguration();
			var remoteConnection = factory.CreateRemoteConnectionConfiguration();
			var signout = factory.CreateSignoutConfiguration();
			var taskManager = factory.CreateTaskManagerConfiguration();
			var userLock = factory.CreateUserLockConfiguration();
			var userSwitch = factory.CreateUserSwitchConfiguration();
			var vmwareOverlay = factory.CreateVmwareOverlayConfiguration();
			var windowsUpdate = factory.CreateWindowsUpdateConfiguration();

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

			return OperationResult.Success;
		}

		public override OperationResult Revert()
		{
			var configurations = backup.GetConfigurations();

			foreach (var configuration in configurations)
			{
				configuration.Restore();
				backup.Delete(configuration);
			}

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
