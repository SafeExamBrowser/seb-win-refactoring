/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Configuration;

namespace SafeExamBrowser.Configuration
{
	[Serializable]
	public class RuntimeInfo : IRuntimeInfo
	{
		public string AppDataFolder { get; set; }
		public DateTime ApplicationStartTime { get; set; }
		public string BrowserCachePath { get; set; }
		public string BrowserLogFile { get; set; }
		public string ClientLogFile { get; set; }
		public string DefaultSettingsFileName { get; set; }
		public string ProgramCopyright { get; set; }
		public string ProgramDataFolder { get; set; }
		public string ProgramTitle { get; set; }
		public string ProgramVersion { get; set; }
		public string RuntimeLogFile { get; set; }
	}
}
