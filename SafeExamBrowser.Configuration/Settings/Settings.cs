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
	internal class Settings : ISettings
	{
		public ConfigurationMode ConfigurationMode { get; set; }
		public ServicePolicy ServicePolicy { get; set; }

		public IBrowserSettings Browser { get; set; }
		public IKeyboardSettings Keyboard { get; set; }
		public IMouseSettings Mouse { get; set; }
		public ITaskbarSettings Taskbar { get; set; }

		public Settings()
		{
			Browser = new BrowserSettings();
			Keyboard = new KeyboardSettings();
			Mouse = new MouseSettings();
			Taskbar = new TaskbarSettings();
		}
	}
}
