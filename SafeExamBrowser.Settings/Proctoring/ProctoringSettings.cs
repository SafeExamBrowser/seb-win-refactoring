/*
 * Copyright (c) 2025 ETH Zürich, IT Services
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
		/// Determines whether the message input for the raise hand notification will be forced.
		/// </summary>
		public bool ForceRaiseHandMessage { get; set; }

		/// <summary>
		/// All settings for the screen proctoring.
		/// </summary>
		public ScreenProctoringSettings ScreenProctoring { get; set; }

		/// <summary>
		/// Determines whether the raise hand notification will be shown in the shell.
		/// </summary>
		public bool ShowRaiseHandNotification { get; set; }

		/// <summary>
		/// Determines whether the proctoring notification will be shown in the taskbar.
		/// </summary>
		public bool ShowTaskbarNotification { get; set; }

		public ProctoringSettings()
		{
			ScreenProctoring = new ScreenProctoringSettings();
		}
	}
}
