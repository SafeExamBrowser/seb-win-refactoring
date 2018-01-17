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
	internal class BrowserSettings : IBrowserSettings
	{
		public bool AllowAddressBar => true;
		public bool AllowBackwardNavigation => true;
		public bool AllowDeveloperConsole => true;
		public bool AllowForwardNavigation => true;
		public bool AllowReloading => true;
		public string CachePath { get; set; }
		public bool FullScreenMode => false;
		public string LogFile { get; set; }
		public string StartUrl => "www.duckduckgo.com";
	}
}
