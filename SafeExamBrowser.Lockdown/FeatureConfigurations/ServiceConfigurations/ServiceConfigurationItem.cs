/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Lockdown.FeatureConfigurations.ServiceConfigurations
{
	internal class ServiceConfigurationItem
	{
		internal ServiceStatus Disabled { get; }
		internal ServiceStatus Enabled { get; }
		internal string Name { get; }

		internal ServiceConfigurationItem(string name, ServiceStatus disabled, ServiceStatus enabled)
		{
			Disabled = disabled;
			Enabled = enabled;
			Name = name;
		}
	}
}
