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
	internal class ChromeNotificationConfiguration : FeatureConfiguration
	{
		public ChromeNotificationConfiguration(Guid groupId, ILogger logger) : base(groupId, logger)
		{
		}

		public override bool DisableFeature()
		{
			logger.Info("Disabling...");

			return true;
		}

		public override bool EnableFeature()
		{
			logger.Info("Enabling...");

			return true;
		}

		public override void Monitor()
		{
			logger.Info("Monitoring...");
		}

		public override bool Restore()
		{
			logger.Info("Restoring...");

			return true;
		}
	}
}
