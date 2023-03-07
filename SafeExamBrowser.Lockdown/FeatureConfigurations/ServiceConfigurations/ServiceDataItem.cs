/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Lockdown.FeatureConfigurations.ServiceConfigurations
{
	[Serializable]
	internal class ServiceDataItem
	{
		internal string Name { get; set; }
		internal ServiceStatus Status { get; set; }

		public override string ToString()
		{
			return $@"'{Name}' => '{(Status == ServiceStatus.Running ? ServiceStatus.Running.ToString().ToLower() : ServiceStatus.Stopped.ToString().ToLower())}'";
		}
	}
}
