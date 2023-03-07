/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Lockdown.FeatureConfigurations.RegistryConfigurations
{
	internal class RegistryConfigurationItem
	{
		internal object Disabled { get; }
		internal object Enabled { get; }
		internal string Key { get; }
		internal string Value { get; }

		internal RegistryConfigurationItem(string key, string value, object disabled, object enabled)
		{
			Key = key;
			Value = value;
			Disabled = disabled;
			Enabled = enabled;
		}
	}
}
