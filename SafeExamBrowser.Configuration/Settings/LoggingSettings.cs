/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Configuration.Settings;

namespace SafeExamBrowser.Configuration.Settings
{
	[Serializable]
	internal class LoggingSettings : ILoggingSettings
	{
		public DateTime ApplicationStartTime { get; set; }
		public string BrowserLogFile { get; set; }
		public string ClientLogFile { get; set; }
		public string RuntimeLogFile { get; set; }
	}
}
