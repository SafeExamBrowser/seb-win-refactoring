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

		public IFeatureConfiguration CreateChromeNotificationConfiguration(Guid groupId, string sid, string userName)
		{
			return new ChromeNotificationConfiguration(groupId, logger.CloneFor(nameof(ChromeNotificationConfiguration)), sid, userName);
		}

		public IFeatureConfiguration CreateEaseOfAccessConfiguration(Guid groupId)
		{
			return new EaseOfAccessConfiguration(groupId, logger.CloneFor(nameof(EaseOfAccessConfiguration)));
		}

		public IFeatureConfiguration CreateNetworkOptionsConfiguration(Guid groupId)
		{
			return new NetworkOptionsConfiguration(groupId, logger.CloneFor(nameof(NetworkOptionsConfiguration)));
		}

		public IFeatureConfiguration CreatePasswordChangeConfiguration(Guid groupId)
		{
			return new PasswordChangeConfiguration(groupId, logger.CloneFor(nameof(PasswordChangeConfiguration)));
		}

		public IFeatureConfiguration CreatePowerOptionsConfiguration(Guid groupId)
		{
			return new PowerOptionsConfiguration(groupId, logger.CloneFor(nameof(PowerOptionsConfiguration)));
		}

		public IFeatureConfiguration CreateRemoteConnectionConfiguration(Guid groupId)
		{
			return new RemoteConnectionConfiguration(groupId, logger.CloneFor(nameof(RemoteConnectionConfiguration)));
		}

		public IFeatureConfiguration CreateSignoutConfiguration(Guid groupId)
		{
			return new SignoutConfiguration(groupId, logger.CloneFor(nameof(SignoutConfiguration)));
		}

		public IFeatureConfiguration CreateTaskManagerConfiguration(Guid groupId)
		{
			return new TaskManagerConfiguration(groupId, logger.CloneFor(nameof(TaskManagerConfiguration)));
		}

		public IFeatureConfiguration CreateUserLockConfiguration(Guid groupId)
		{
			return new UserLockConfiguration(groupId, logger.CloneFor(nameof(UserLockConfiguration)));
		}

		public IFeatureConfiguration CreateUserSwitchConfiguration(Guid groupId)
		{
			return new UserSwitchConfiguration(groupId, logger.CloneFor(nameof(UserSwitchConfiguration)));
		}

		public IFeatureConfiguration CreateVmwareOverlayConfiguration(Guid groupId)
		{
			return new VmwareOverlayConfiguration(groupId, logger.CloneFor(nameof(VmwareOverlayConfiguration)));
		}

		public IFeatureConfiguration CreateWindowsUpdateConfiguration(Guid groupId)
		{
			return new WindowsUpdateConfiguration(groupId, logger.CloneFor(nameof(WindowsUpdateConfiguration)));
		}
	}
}
