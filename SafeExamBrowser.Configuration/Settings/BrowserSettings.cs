/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using SafeExamBrowser.Contracts.Configuration.Settings;

namespace SafeExamBrowser.Configuration.Settings
{
	public class BrowserSettings : IBrowserSettings
	{
		private ISettings settings;

		public BrowserSettings(ISettings settings)
		{
			this.settings = settings;
		}

		public bool AllowAddressBar => true;

		public bool AllowBackwardNavigation => false;

		public bool AllowDebugConsole => true;

		public bool AllowForwardNavigation => false;

		public bool AllowReloading => false;

		public string CachePath
		{
			get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), settings.AppDataFolderName, "Cache"); }
		}

		public bool FullScreenMode => throw new NotImplementedException();

		public string LogFile
		{
			get { return Path.Combine(settings.LogFolderPath, $"{settings.RuntimeIdentifier}_Browser.txt"); }
		}
	}
}
