/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
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
		/// Determines whether the user can use the chat.
		/// </summary>
		public bool AllowChat { get; set; }

		/// <summary>
		/// Determines whether the user can use close captions.
		/// </summary>
		public bool AllowClosedCaptions { get; set; }

		/// <summary>
		/// Determines whether the user can use the raise hand feature.
		/// </summary>
		public bool AllowRaiseHand { get; set; }

		/// <summary>
		/// Determines whether the user can record the meeting.
		/// </summary>
		public bool AllowRecording { get; set; }

		/// <summary>
		/// Determines whether the user may use the tile view.
		/// </summary>
		public bool AllowTileView { get; set; }

		/// <summary>
		/// Determines whether the audio starts muted.
		/// </summary>
		public bool AudioMuted { get; set; }

		/// <summary>
		/// Determines whether the meeting runs in an audio-only mode.
		/// </summary>
		public bool AudioOnly { get; set; }

		/// <summary>
		/// Determines whether proctoring with Jitsi Meet is enabled.
		/// </summary>
		public bool Enabled { get; set; }

		/// <summary>
		/// Determines whether the user may receive the video stream of other meeting participants.
		/// </summary>
		public bool ReceiveAudio { get; set; }

		/// <summary>
		/// Determines whether the user may receive the audio stream of other meeting participants.
		/// </summary>
		public bool ReceiveVideo { get; set; }

		/// <summary>
		/// The name of the meeting room.
		/// </summary>
		public string RoomName { get; set; }

		/// <summary>
		/// Determines whether the audio stream of the user will be sent to the server.
		/// </summary>
		public bool SendAudio { get; set; }

		/// <summary>
		/// Determines whether the video stream of the user will be sent to the server.
		/// </summary>
		public bool SendVideo { get; set; }

		/// <summary>
		/// The URL of the Jitsi Meet server.
		/// </summary>
		public string ServerUrl { get; set; }

		/// <summary>
		/// Determines whether the subject will be shown as meeting name.
		/// </summary>
		public bool ShowMeetingName { get; set; }

		/// <summary>
		/// The subject of the meeting.
		/// </summary>
		public string Subject { get; set; }

		/// <summary>
		/// The authentication token for the meeting.
		/// </summary>
		public string Token { get; set; }

		/// <summary>
		/// Determines whether the video starts muted.
		/// </summary>
		public bool VideoMuted { get; set; }
	}
}
