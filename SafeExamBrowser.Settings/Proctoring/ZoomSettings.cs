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
		/// Determines whether the user can use the chat.
		/// </summary>
		public bool AllowChat { get; set; }

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
		public string MeetingNumber { get; set; }

		/// <summary>
		/// The password of the meeting.
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// Determines whether the user may receive the video stream of other meeting participants.
		/// </summary>
		public bool ReceiveAudio { get; set; }

		/// <summary>
		/// Determines whether the user may receive the audio stream of other meeting participants.
		/// </summary>
		public bool ReceiveVideo { get; set; }

		/// <summary>
		/// The signature to be used for authentication.
		/// </summary>
		public string Signature { get; set; }

		/// <summary>
		/// The subject of the meeting.
		/// </summary>
		public string Subject { get; set; }

		/// <summary>
		/// The user name to be used for the meeting.
		/// </summary>
		public string UserName { get; set; }
	}
}
