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

namespace SafeExamBrowser.Lockdown.FeatureConfigurations.RegistryConfigurations.MachineHive
{
	[Serializable]
	internal abstract class MachineHiveConfiguration : RegistryConfiguration
	{
		protected override RegistryKey RootKey => Registry.LocalMachine;

		public MachineHiveConfiguration(Guid groupId, ILogger logger) : base(groupId, logger)
		{
		}

		protected override bool IsHiveAvailable(RegistryDataItem item)
		{
			return true;
		}
	}
}
