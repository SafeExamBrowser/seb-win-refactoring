/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.IO;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Configuration.DataFormats
{
	public class XmlFormat : IDataFormat
	{
		private ILogger logger;

		public XmlFormat(ILogger logger)
		{
			this.logger = logger;
		}

		public bool CanParse(Stream data)
		{
			return false;
		}

		public LoadStatus TryParse(Stream data, out Settings settings, string adminPassword = null, string settingsPassword = null)
		{
			throw new System.NotImplementedException();
		}
	}
}
