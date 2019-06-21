/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Contracts.Lockdown;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Lockdown
{
	public class FeatureConfigurationBackup : IFeatureConfigurationBackup
	{
		private ILogger logger;

		public FeatureConfigurationBackup(ILogger logger)
		{
			this.logger = logger;
		}

		public void Delete(IFeatureConfiguration configuration)
		{
			
		}

		public IList<IFeatureConfiguration> GetConfigurations()
		{
			return new List<IFeatureConfiguration>();
		}

		public void Save(IFeatureConfiguration configuration)
		{
			
		}
	}
}
