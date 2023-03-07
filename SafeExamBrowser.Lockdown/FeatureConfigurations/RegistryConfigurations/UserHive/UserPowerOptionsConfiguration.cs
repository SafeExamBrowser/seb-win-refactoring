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

namespace SafeExamBrowser.Lockdown.FeatureConfigurations.RegistryConfigurations.UserHive
{
	[Serializable]
	internal class UserPowerOptionsConfiguration : UserHiveConfiguration
	{
		protected override IEnumerable<RegistryConfigurationItem> Items => new[]
		{
			new RegistryConfigurationItem($@"HKEY_USERS\{SID}\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoClose", 1, 0)
		};

		public UserPowerOptionsConfiguration(Guid groupId, ILogger logger, string sid, string userName) : base(groupId, logger, sid, userName)
		{
		}

		public override bool Reset()
		{
			return DeleteConfiguration();
		}
	}
}
