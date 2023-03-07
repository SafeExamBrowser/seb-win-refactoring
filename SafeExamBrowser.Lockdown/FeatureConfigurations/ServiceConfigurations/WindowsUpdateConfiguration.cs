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

namespace SafeExamBrowser.Lockdown.FeatureConfigurations.ServiceConfigurations
{
	[Serializable]
	internal class WindowsUpdateConfiguration : ServiceConfiguration
	{
		protected override IEnumerable<ServiceConfigurationItem> Items => new []
		{
			new ServiceConfigurationItem("wuauserv", ServiceStatus.Stopped, ServiceStatus.Running)
		};

		public WindowsUpdateConfiguration(Guid groupId, ILogger logger) : base(groupId, logger)
		{
		}

		public override bool Reset()
		{
			return EnableFeature();
		}
	}
}
