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
		public string AppDataFolder { get; set; }
		public string ProgramCopyright { get; set; }
		public string ProgramTitle { get; set; }
		public string ProgramVersion { get; set; }

		public IBrowserSettings Browser { get; private set; }
		public IKeyboardSettings Keyboard { get; private set; }
		public ILoggingSettings Logging { get; private set; }
		public IMouseSettings Mouse { get; private set; }
		public ITaskbarSettings Taskbar { get; private set; }

		public Settings(BrowserSettings browser, KeyboardSettings keyboard, LoggingSettings logging, MouseSettings mouse, TaskbarSettings taskbar)
		{
			Browser = browser;
			Keyboard = keyboard;
			Logging = logging;
			Mouse = mouse;
			Taskbar = taskbar;
		}
	}
}
