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
		public IBrowserSettings Browser { get; private set; }
		public IKeyboardSettings Keyboard { get; private set; }
		public IMouseSettings Mouse { get; private set; }
		public ITaskbarSettings Taskbar { get; private set; }

		public Settings(BrowserSettings browser, KeyboardSettings keyboard, MouseSettings mouse, TaskbarSettings taskbar)
		{
			Browser = browser;
			Keyboard = keyboard;
			Mouse = mouse;
			Taskbar = taskbar;
		}
	}
}
