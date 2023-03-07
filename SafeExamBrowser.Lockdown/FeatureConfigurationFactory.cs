/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Lockdown.Contracts;
using SafeExamBrowser.Lockdown.FeatureConfigurations.RegistryConfigurations.MachineHive;
using SafeExamBrowser.Lockdown.FeatureConfigurations.RegistryConfigurations.UserHive;
using SafeExamBrowser.Lockdown.FeatureConfigurations.ServiceConfigurations;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Lockdown
{
	public class FeatureConfigurationFactory : IFeatureConfigurationFactory
	{
		private readonly IModuleLogger logger;

		public FeatureConfigurationFactory(IModuleLogger logger)
		{
			this.logger = logger;
		}

		public IList<IFeatureConfiguration> CreateAll(Guid groupId, string sid, string userName)
		{
			return new List<IFeatureConfiguration>
			{
				CreateChangePasswordConfiguration(groupId, sid, userName),
				CreateChromeNotificationConfiguration(groupId, sid, userName),
				CreateEaseOfAccessConfiguration(groupId),
				CreateFindPrinterConfiguration(groupId, sid, userName),
				CreateLockWorkstationConfiguration(groupId, sid, userName),
				CreateMachinePowerOptionsConfiguration(groupId),
				CreateNetworkOptionsConfiguration(groupId),
				CreateRemoteConnectionConfiguration(groupId),
				CreateSignoutConfiguration(groupId, sid, userName),
				CreateSwitchUserConfiguration(groupId),
				CreateTaskManagerConfiguration(groupId, sid, userName),
				CreateUserPowerOptionsConfiguration(groupId, sid, userName),
				CreateVmwareOverlayConfiguration(groupId, sid, userName),
				CreateWindowsUpdateConfiguration(groupId)
			};
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

		public IFeatureConfiguration CreateFindPrinterConfiguration(Guid groupId, string sid, string userName)
		{
			return new FindPrinterConfiguration(groupId, logger.CloneFor(nameof(FindPrinterConfiguration)), sid, userName);
		}

		public IFeatureConfiguration CreateLockWorkstationConfiguration(Guid groupId, string sid, string userName)
		{
			return new LockWorkstationConfiguration(groupId, logger.CloneFor(nameof(LockWorkstationConfiguration)), sid, userName);
		}

		public IFeatureConfiguration CreateMachinePowerOptionsConfiguration(Guid groupId)
		{
			return new MachinePowerOptionsConfiguration(groupId, logger.CloneFor(nameof(MachinePowerOptionsConfiguration)));
		}

		public IFeatureConfiguration CreateNetworkOptionsConfiguration(Guid groupId)
		{
			return new NetworkOptionsConfiguration(groupId, logger.CloneFor(nameof(NetworkOptionsConfiguration)));
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

		public IFeatureConfiguration CreateUserPowerOptionsConfiguration(Guid groupId, string sid, string userName)
		{
			return new UserPowerOptionsConfiguration(groupId, logger.CloneFor(nameof(UserPowerOptionsConfiguration)), sid, userName);
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
