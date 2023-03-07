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
	/// <summary>
	/// IMPORTANT: This registry configuration only has an effect after Chrome is restarted!
	/// 
	/// See https://www.chromium.org/administrators/policy-list-3#DefaultNotificationsSetting:
	/// •	1 = Allow sites to show desktop notifications.
	/// •	2 = Do not allow any site to show desktop notifications.
	/// •	3 = Ask every time a site wants to show desktop notifications.
	/// </summary>
	[Serializable]
	internal class ChromeNotificationConfiguration : UserHiveConfiguration
	{
		protected override IEnumerable<RegistryConfigurationItem> Items => new []
		{
			new RegistryConfigurationItem($@"HKEY_USERS\{SID}\Software\Policies\Google\Chrome", "DefaultNotificationsSetting", 2, 1)
		};

		public ChromeNotificationConfiguration(Guid groupId, ILogger logger, string sid, string userName) : base(groupId, logger, sid, userName)
		{
		}

		public override bool Reset()
		{
			return DeleteConfiguration();
		}
	}
}
