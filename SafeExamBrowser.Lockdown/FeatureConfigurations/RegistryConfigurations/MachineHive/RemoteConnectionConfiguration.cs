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
	/// <summary>
	/// Specifies whether Remote Desktop connections are enabled.
	/// 
	/// See https://docs.microsoft.com/en-us/windows-hardware/customize/desktop/unattend/microsoft-windows-terminalservices-localsessionmanager-fdenytsconnections:
	///		0 = Specifies that remote desktop connections are enabled.
	///		1 = Specifies that remote desktop connections are denied. This is the default value.
	/// </summary>
	[Serializable]
	internal class RemoteConnectionConfiguration : MachineHiveConfiguration
	{
		protected override IEnumerable<RegistryConfigurationItem> Items => new[]
		{
			new RegistryConfigurationItem(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Terminal Server", "fDenyTSConnections", 1, 0)
		};

		public RemoteConnectionConfiguration(Guid groupId, ILogger logger) : base(groupId, logger)
		{
		}

		public override bool Reset()
		{
			return DisableFeature();
		}
	}
}
