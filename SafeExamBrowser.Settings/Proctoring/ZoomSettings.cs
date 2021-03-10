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
	/// All settings for the meeting provider Zoom.
	/// </summary>
	[Serializable]
	public class ZoomSettings
	{
		/// <summary>
		/// The API key to be used for authentication.
		/// </summary>
		public string ApiKey { get; set; }

		/// <summary>
		/// The API secret to be used for authentication.
		/// </summary>
		public string ApiSecret { get; set; }

		/// <summary>
		/// Determines whether proctoring with Zoom is enabled.
		/// </summary>
		public bool Enabled { get; set; }

		/// <summary>
		/// The number of the meeting.
		/// </summary>
		public int MeetingNumber { get; set; }

		/// <summary>
		/// The user name to be used for the meeting.
		/// </summary>
		public string UserName { get; set; }
	}
}
