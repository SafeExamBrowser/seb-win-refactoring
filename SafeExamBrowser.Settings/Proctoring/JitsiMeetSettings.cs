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
	/// All settings for the meeting provider Jitsi Meet.
	/// </summary>
	[Serializable]
	public class JitsiMeetSettings
	{
		/// <summary>
		/// Determines whether proctoring with Jitsi Meet is enabled.
		/// </summary>
		public bool Enabled { get; set; }

		/// <summary>
		/// The name of the meeting room.
		/// </summary>
		public string RoomName { get; set; }

		/// <summary>
		/// The URL of the Jitsi Meet server.
		/// </summary>
		public string ServerUrl { get; set; }

		/// <summary>
		/// The subject of the meeting.
		/// </summary>
		public string Subject { get; set; }

		/// <summary>
		/// The authentication token for the meeting.
		/// </summary>
		public string Token { get; set; }
	}
}
