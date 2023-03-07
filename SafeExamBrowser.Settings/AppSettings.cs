/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Settings.Applications;
using SafeExamBrowser.Settings.Browser;
using SafeExamBrowser.Settings.Logging;
using SafeExamBrowser.Settings.Monitoring;
using SafeExamBrowser.Settings.Proctoring;
using SafeExamBrowser.Settings.Security;
using SafeExamBrowser.Settings.Server;
using SafeExamBrowser.Settings.Service;
using SafeExamBrowser.Settings.SystemComponents;
using SafeExamBrowser.Settings.UserInterface;

namespace SafeExamBrowser.Settings
{
	/// <summary>
	/// Defines all settings for the application.
	/// </summary>
	[Serializable]
	public class AppSettings
	{
		/// <summary>
		/// All action center-related settings.
		/// </summary>
		public ActionCenterSettings ActionCenter { get; set; }

		/// <summary>
		/// All settings related to external applications.
		/// </summary>
		public ApplicationSettings Applications { get; set; }

		/// <summary>
		/// All audio-related settings.
		/// </summary>
		public AudioSettings Audio { get; set; }

		/// <summary>
		/// All browser-related settings.
		/// </summary>
		public BrowserSettings Browser { get; set; }

		/// <summary>
		/// The mode which determines the configuration behaviour.
		/// </summary>
		public ConfigurationMode ConfigurationMode { get; set; }

		/// <summary>
		/// All display-related settings.
		/// </summary>
		public DisplaySettings Display { get; set; }

		/// <summary>
		/// All keyboard-related settings.
		/// </summary>
		public KeyboardSettings Keyboard { get; set; }

		/// <summary>
		/// The global log severity to be used.
		/// </summary>
		public LogLevel LogLevel { get; set; }

		/// <summary>
		/// All mouse-related settings.
		/// </summary>
		public MouseSettings Mouse { get; set; }

		/// <summary>
		/// All proctoring-related settings.
		/// </summary>
		public ProctoringSettings Proctoring { get; set; }

		/// <summary>
		/// All security-related settings.
		/// </summary>
		public SecuritySettings Security { get; set; }

		/// <summary>
		/// All server-related settings.
		/// </summary>
		public ServerSettings Server { get; set; }

		/// <summary>
		/// All service-related settings.
		/// </summary>
		public ServiceSettings Service { get; set; }

		/// <summary>
		/// The mode which determines the session behaviour.
		/// </summary>
		public SessionMode SessionMode { get; set; }

		/// <summary>
		/// All taskbar-related settings.
		/// </summary>
		public TaskbarSettings Taskbar { get; set; }

		/// <summary>
		/// The mode which determines the look &amp; feel of the user interface.
		/// </summary>
		public UserInterfaceMode UserInterfaceMode { get; set; }

		public AppSettings()
		{
			ActionCenter = new ActionCenterSettings();
			Applications = new ApplicationSettings();
			Audio = new AudioSettings();
			Browser = new BrowserSettings();
			Display = new DisplaySettings();
			Keyboard = new KeyboardSettings();
			Mouse = new MouseSettings();
			Proctoring = new ProctoringSettings();
			Security = new SecuritySettings();
			Server = new ServerSettings();
			Service = new ServiceSettings();
			Taskbar = new TaskbarSettings();
		}
	}
}
