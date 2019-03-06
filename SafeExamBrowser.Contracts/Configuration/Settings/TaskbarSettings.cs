/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Contracts.Configuration.Settings
{
	/// <summary>
	/// Defines all configuration options for the <see cref="UserInterface.Shell.ITaskbar"/>.
	/// </summary>
	[Serializable]
	public class TaskbarSettings
	{
		/// <summary>
		/// Determines whether the user may switch the keyboard layout during runtime.
		/// </summary>
		public bool AllowKeyboardLayout { get; set; }

		/// <summary>
		/// Determines whether the user may access the application log during runtime.
		/// </summary>
		public bool AllowApplicationLog { get; set; }

		/// <summary>
		/// Determines whether the user may control the wireless network connection during runtime.
		/// </summary>
		public bool AllowWirelessNetwork { get; set; }

		/// <summary>
		/// Determines whether the taskbar itself is enabled and visible to the user.
		/// </summary>
		public bool EnableTaskbar { get; set; }

		/// <summary>
		/// Determines whether the current date and time will be rendered in the taskbar.
		/// </summary>
		public bool ShowClock { get; set; }
	}
}
