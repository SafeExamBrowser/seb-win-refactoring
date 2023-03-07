/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Lockdown.FeatureConfigurations.RegistryConfigurations
{
	[Serializable]
	internal class RegistryDataItem
	{
		internal object Data { get; set; }
		internal string Key { get; set; }
		internal string Value { get; set; }

		public override string ToString()
		{
			return $@"'{Key}\{Value}'{(Data != null ? $" => '{Data}'" : "")}";
		}
	}
}
