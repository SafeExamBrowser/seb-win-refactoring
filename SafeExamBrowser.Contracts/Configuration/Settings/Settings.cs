/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Contracts.Configuration.Settings
{
	/// <summary>
	/// Defines all configuration options for the application.
	/// </summary>
	[Serializable]
	public class Settings
	{
		/// <summary>
		/// The hash code of the administrator password for the settings.
		/// </summary>
		public string AdminPasswordHash { get; set; }

		/// <summary>
		/// The mode which determines the configuration behaviour.
		/// </summary>
		public ConfigurationMode ConfigurationMode { get; set; }

		/// <summary>
		/// All browser-related settings.
		/// </summary>
		public BrowserSettings Browser { get; set; }

		/// <summary>
		/// All keyboard-related settings.
		/// </summary>
		public KeyboardSettings Keyboard { get; set; }

		/// <summary>
		/// The kiosk mode which determines how the computer is locked down.
		/// </summary>
		public KioskMode KioskMode { get; set; }

		/// <summary>
		/// All mouse-related settings.
		/// </summary>
		public MouseSettings Mouse { get; set; }

		/// <summary>
		/// The active policy for the service component.
		/// </summary>
		public ServicePolicy ServicePolicy { get; set; }

		/// <summary>
		/// All taskbar-related settings.
		/// </summary>
		public TaskbarSettings Taskbar { get; set; }

		public Settings()
		{
			Browser = new BrowserSettings();
			Keyboard = new KeyboardSettings();
			Mouse = new MouseSettings();
			Taskbar = new TaskbarSettings();

			// TODO: For version 3.0 Alpha only, remove for final release!
			ServicePolicy = ServicePolicy.Optional;
			Taskbar.AllowApplicationLog = true;
		}
	}
}
