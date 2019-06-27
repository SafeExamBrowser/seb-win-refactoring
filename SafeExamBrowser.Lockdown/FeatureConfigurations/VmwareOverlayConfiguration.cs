/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Lockdown.FeatureConfigurations
{
	[Serializable]
	internal class VmwareOverlayConfiguration : FeatureConfiguration
	{
		public VmwareOverlayConfiguration(Guid groupId, ILogger logger) : base(groupId, logger)
		{
		}

		public override bool DisableFeature()
		{
			return true;
		}

		public override bool EnableFeature()
		{
			return true;
		}

		public override void Monitor()
		{

		}

		public override bool Restore()
		{
			return true;
		}
	}
}
