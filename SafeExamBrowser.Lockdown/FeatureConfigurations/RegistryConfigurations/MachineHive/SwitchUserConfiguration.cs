/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Lockdown.FeatureConfigurations.RegistryConfigurations.MachineHive
{
	[Serializable]
	internal class SwitchUserConfiguration : MachineHiveConfiguration
	{
		protected override IEnumerable<RegistryConfigurationItem> Items => new []
		{
			new RegistryConfigurationItem(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Policies\System", "HideFastUserSwitching", 1, 0)
		};

		public SwitchUserConfiguration(Guid groupId, ILogger logger) : base(groupId, logger)
		{
		}

		public override bool Reset()
		{
			return DeleteConfiguration();
		}
	}
}
