/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Lockdown;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Lockdown.FeatureConfigurations;

namespace SafeExamBrowser.Lockdown
{
	public class FeatureConfigurationFactory : IFeatureConfigurationFactory
	{
		private IModuleLogger logger;

		public FeatureConfigurationFactory(IModuleLogger logger)
		{
			this.logger = logger;
		}

		public IFeatureConfiguration CreateChromeNotificationConfiguration()
		{
			return new ChromeNotificationConfiguration();
		}

		public IFeatureConfiguration CreateEaseOfAccessConfiguration()
		{
			return new EaseOfAccessConfiguration();
		}

		public IFeatureConfiguration CreateNetworkOptionsConfiguration()
		{
			return new NetworkOptionsConfiguration();
		}

		public IFeatureConfiguration CreatePasswordChangeConfiguration()
		{
			return new PasswordChangeConfiguration();
		}

		public IFeatureConfiguration CreatePowerOptionsConfiguration()
		{
			return new PowerOptionsConfiguration();
		}

		public IFeatureConfiguration CreateRemoteConnectionConfiguration()
		{
			return new RemoteConnectionConfiguration();
		}

		public IFeatureConfiguration CreateSignoutConfiguration()
		{
			return new SignoutConfiguration();
		}

		public IFeatureConfiguration CreateTaskManagerConfiguration()
		{
			return new TaskManagerConfiguration();
		}

		public IFeatureConfiguration CreateUserLockConfiguration()
		{
			return new UserLockConfiguration();
		}

		public IFeatureConfiguration CreateUserSwitchConfiguration()
		{
			return new UserSwitchConfiguration();
		}

		public IFeatureConfiguration CreateVmwareOverlayConfiguration()
		{
			return new VmwareOverlayConfiguration();
		}

		public IFeatureConfiguration CreateWindowsUpdateConfiguration()
		{
			return new WindowsUpdateConfiguration();
		}
	}
}
