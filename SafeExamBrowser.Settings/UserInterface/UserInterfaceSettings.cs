/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Settings.UserInterface
{
	/// <summary>
	/// Defines all settings for the user interface.
	/// </summary>
	[Serializable]
	public class UserInterfaceSettings
	{
		/// <summary>
		/// All settings related to the action center.
		/// </summary>
		public ActionCenterSettings ActionCenter { get; set; }

		/// <summary>
		/// All settings related to the lock screen.
		/// </summary>
		public LockScreenSettings LockScreen { get; set; }

		/// <summary>
		/// The mode which determines the look &amp; feel of the user interface.
		/// </summary>
		public UserInterfaceMode Mode { get; set; }

		/// <summary>
		/// All taskbar-related settings.
		/// </summary>
		public TaskbarSettings Taskbar { get; set; }

		public UserInterfaceSettings()
		{
			ActionCenter = new ActionCenterSettings();
			LockScreen = new LockScreenSettings();
			Taskbar = new TaskbarSettings();
		}
	}
}
