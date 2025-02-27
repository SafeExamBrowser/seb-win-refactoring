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
	/// All settings related to the metadata capturing of the screen proctoring.
	/// </summary>
	[Serializable]
	public class MetaDataSettings
	{
		/// <summary>
		/// Determines whether data of the active application shall be captured and transmitted.
		/// </summary>
		public bool CaptureApplicationData { get; set; }

		/// <summary>
		/// Determines whether data of the browser application shall be captured and transmitted.
		/// </summary>
		public bool CaptureBrowserData { get; set; }

		/// <summary>
		/// Determines whether the title of the currently active window shall be captured and transmitted.
		/// </summary>
		public bool CaptureWindowTitle { get; set; }
	}
}
