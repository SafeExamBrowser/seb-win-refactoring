/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Lockdown;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Lockdown.FeatureConfigurations;
using SafeExamBrowser.Lockdown.FeatureConfigurations.RegistryConfigurations.MachineHive;
using SafeExamBrowser.Lockdown.FeatureConfigurations.RegistryConfigurations.UserHive;

namespace SafeExamBrowser.Lockdown
{
	public class FeatureConfigurationFactory : IFeatureConfigurationFactory
	{
		private IModuleLogger logger;

		public FeatureConfigurationFactory(IModuleLogger logger)
		{
			this.logger = logger;
		}

		public IFeatureConfiguration CreateChangePasswordConfiguration(Guid groupId, string sid, string userName)
		{
			return new ChangePasswordConfiguration(groupId, logger.CloneFor(nameof(ChangePasswordConfiguration)), sid, userName);
		}

		public IFeatureConfiguration CreateChromeNotificationConfiguration(Guid groupId, string sid, string userName)
		{
			return new ChromeNotificationConfiguration(groupId, logger.CloneFor(nameof(ChromeNotificationConfiguration)), sid, userName);
		}

		public IFeatureConfiguration CreateEaseOfAccessConfiguration(Guid groupId)
		{
			return new EaseOfAccessConfiguration(groupId, logger.CloneFor(nameof(EaseOfAccessConfiguration)));
		}

		public IFeatureConfiguration CreateLockWorkstationConfiguration(Guid groupId, string sid, string userName)
		{
			return new LockWorkstationConfiguration(groupId, logger.CloneFor(nameof(LockWorkstationConfiguration)), sid, userName);
		}

		public IFeatureConfiguration CreateNetworkOptionsConfiguration(Guid groupId)
		{
			return new NetworkOptionsConfiguration(groupId, logger.CloneFor(nameof(NetworkOptionsConfiguration)));
		}

		public IFeatureConfiguration CreatePowerOptionsConfiguration(Guid groupId)
		{
			return new PowerOptionsConfiguration(groupId, logger.CloneFor(nameof(PowerOptionsConfiguration)));
		}

		public IFeatureConfiguration CreateRemoteConnectionConfiguration(Guid groupId)
		{
			return new RemoteConnectionConfiguration(groupId, logger.CloneFor(nameof(RemoteConnectionConfiguration)));
		}

		public IFeatureConfiguration CreateSignoutConfiguration(Guid groupId, string sid, string userName)
		{
			return new SignoutConfiguration(groupId, logger.CloneFor(nameof(SignoutConfiguration)), sid, userName);
		}

		public IFeatureConfiguration CreateSwitchUserConfiguration(Guid groupId)
		{
			return new SwitchUserConfiguration(groupId, logger.CloneFor(nameof(SwitchUserConfiguration)));
		}

		public IFeatureConfiguration CreateTaskManagerConfiguration(Guid groupId, string sid, string userName)
		{
			return new TaskManagerConfiguration(groupId, logger.CloneFor(nameof(TaskManagerConfiguration)), sid, userName);
		}

		public IFeatureConfiguration CreateVmwareOverlayConfiguration(Guid groupId, string sid, string userName)
		{
			return new VmwareOverlayConfiguration(groupId, logger.CloneFor(nameof(VmwareOverlayConfiguration)), sid, userName);
		}

		public IFeatureConfiguration CreateWindowsUpdateConfiguration(Guid groupId)
		{
			return new WindowsUpdateConfiguration(groupId, logger.CloneFor(nameof(WindowsUpdateConfiguration)));
		}
	}
}
