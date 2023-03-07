/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Settings.UserInterface
{
	/// <summary>
	/// Defines all settings for the action center.
	/// </summary>
	[Serializable]
	public class ActionCenterSettings
	{
		/// <summary>
		/// Determines whether the action center itself is enabled and visible to the user.
		/// </summary>
		public bool EnableActionCenter { get; set; }

		/// <summary>
		/// Determines whether the about window is accessible via the action center.
		/// </summary>
		public bool ShowApplicationInfo { get; set; }

		/// <summary>
		/// Determines whether the application log is accessible via the action center.
		/// </summary>
		public bool ShowApplicationLog { get; set; }

		/// <summary>
		/// Determines whether the system control for audio is accessible via the action center.
		/// </summary>
		public bool ShowAudio { get; set; }

		/// <summary>
		/// Determines whether the current date and time will be rendered in the action center.
		/// </summary>
		public bool ShowClock { get; set; }

		/// <summary>
		/// Determines whether the system control for the keyboard layout is accessible via the action center.
		/// </summary>
		public bool ShowKeyboardLayout { get; set; }

		/// <summary>
		/// Determines whether the system control for the network is accessible via the action center.
		/// </summary>
		public bool ShowNetwork { get; set; }
	}
}
