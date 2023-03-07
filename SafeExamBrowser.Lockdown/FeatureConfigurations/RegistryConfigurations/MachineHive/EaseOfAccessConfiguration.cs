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
	/// Controls whether the ease of access option is available on the Security / Login Screen of the operating system. See also
	/// https://learn.microsoft.com/en-us/windows-hardware/customize/desktop/unattend/microsoft-windows-embedded-embeddedlogon-brandingneutral.
	/// </summary>
	[Serializable]
	internal class EaseOfAccessConfiguration : MachineHiveConfiguration
	{
		protected override IEnumerable<RegistryConfigurationItem> Items => new[]
		{
			new RegistryConfigurationItem(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows Embedded\EmbeddedLogon", "BrandingNeutral", 8, 0),
			new RegistryConfigurationItem(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\Utilman.exe", "Debugger", "SebDummy.exe", "")
		};

		public EaseOfAccessConfiguration(Guid groupId, ILogger logger) : base(groupId, logger)
		{
		}

		public override bool Reset()
		{
			return DeleteConfiguration();
		}
	}
}
