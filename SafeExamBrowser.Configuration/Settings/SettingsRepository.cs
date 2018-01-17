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
	public class SettingsRepository : ISettingsRepository
	{
		public ISettings Load(Uri path)
		{
			// TODO
			throw new NotImplementedException();
		}

		public ISettings LoadDefaults()
		{
			var browser = new BrowserSettings();
			var keyboard = new KeyboardSettings();
			var mouse = new MouseSettings();
			var taskbar = new TaskbarSettings();
			var settings = new Settings(browser, keyboard, mouse, taskbar);
			
			// TODO

			return settings;
		}
	}
}
