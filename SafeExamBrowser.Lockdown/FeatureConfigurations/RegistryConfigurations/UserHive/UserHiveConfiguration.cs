/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.Win32;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Lockdown.FeatureConfigurations.RegistryConfigurations.UserHive
{
	[Serializable]
	internal abstract class UserHiveConfiguration : RegistryConfiguration
	{
		protected string SID { get; }
		protected string UserName { get; }

		protected override RegistryKey RootKey => Registry.Users; 

		public UserHiveConfiguration(Guid groupId, ILogger logger, string sid, string userName) : base(groupId, logger)
		{
			SID = sid ?? throw new ArgumentNullException(nameof(sid));
			UserName = userName ?? throw new ArgumentNullException(nameof(userName));
		}

		public override string ToString()
		{
			return $"{GetType().Name} ({Id}) for user '{UserName}'";
		}

		protected override bool IsHiveAvailable(RegistryDataItem item)
		{
			var isAvailable = false;

			try
			{
				isAvailable = Registry.Users.OpenSubKey(SID) != null;
			}
			catch (Exception e)
			{
				logger.Error($"Failed to check availability of registry hive for item {item}!", e);
			}

			return isAvailable;
		}
	}
}
