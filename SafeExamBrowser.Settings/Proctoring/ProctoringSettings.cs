/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Settings.Proctoring
{
	/// <summary>
	/// Defines all settings related to remote proctoring.
	/// </summary>
	[Serializable]
	public class ProctoringSettings
	{
		/// <summary>
		/// Determines whether the entire remote proctoring feature is enabled.
		/// </summary>
		public bool Enabled { get; set; }

		/// <summary>
		/// All settings for remote proctoring with Jitsi Meet.
		/// </summary>
		public JitsiMeetSettings JitsiMeet { get; set; }

		/// <summary>
		/// Determines the visibility of the proctoring window.
		/// </summary>
		public WindowVisibility WindowVisibility { get; set; }

		/// <summary>
		/// All settings for remote proctoring with Zoom.
		/// </summary>
		public ZoomSettings Zoom { get; set; }

		public ProctoringSettings()
		{
			JitsiMeet = new JitsiMeetSettings();
			Zoom = new ZoomSettings();
		}
	}
}
