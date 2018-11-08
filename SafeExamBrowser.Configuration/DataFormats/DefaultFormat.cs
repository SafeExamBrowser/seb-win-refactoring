/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Configuration.DataFormats
{
	public class DefaultFormat : IDataFormat
	{
		private ILogger logger;

		public DefaultFormat(ILogger logger)
		{
			this.logger = logger;
		}

		public bool CanParse(byte[] data)
		{
			return true;
		}

		public LoadStatus TryParse(byte[] data, out Settings settings, string adminPassword = null, string settingsPassword = null)
		{
			settings = new Settings();
			settings.ServicePolicy = ServicePolicy.Optional;

			return LoadStatus.Success;
		}
	}
}
