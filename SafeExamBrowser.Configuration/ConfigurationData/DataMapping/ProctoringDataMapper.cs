/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.Proctoring;

namespace SafeExamBrowser.Configuration.ConfigurationData.DataMapping
{
	internal class ProctoringDataMapper : BaseDataMapper
	{
		internal override void Map(string key, object value, AppSettings settings)
		{
			switch (key)
			{
				case Keys.Proctoring.ForceRaiseHandMessage:
					MapForceRaiseHandMessage(settings, value);
					break;
				case Keys.Proctoring.JitsiMeet.AllowChat:
					MapJitsiMeetAllowChat(settings, value);
					break;
				case Keys.Proctoring.JitsiMeet.AllowClosedCaptions:
					MapJitsiMeetAllowClosedCaptions(settings, value);
					break;
				case Keys.Proctoring.JitsiMeet.AllowRaiseHand:
					MapJitsiMeetAllowRaiseHands(settings, value);
					break;
				case Keys.Proctoring.JitsiMeet.AllowRecording:
					MapJitsiMeetAllowRecording(settings, value);
					break;
				case Keys.Proctoring.JitsiMeet.AllowTileView:
					MapJitsiMeetAllowTileView(settings, value);
					break;
				case Keys.Proctoring.JitsiMeet.AudioMuted:
					MapJitsiMeetAudioMuted(settings, value);
					break;
				case Keys.Proctoring.JitsiMeet.AudioOnly:
					MapJitsiMeetAudioOnly(settings, value);
					break;
				case Keys.Proctoring.JitsiMeet.Enabled:
					MapJitsiMeetEnabled(settings, value);
					break;
				case Keys.Proctoring.JitsiMeet.ReceiveAudio:
					MapJitsiMeetReceiveAudio(settings, value);
					break;
				case Keys.Proctoring.JitsiMeet.ReceiveVideo:
					MapJitsiMeetReceiveVideo(settings, value);
					break;
				case Keys.Proctoring.JitsiMeet.RoomName:
					MapJitsiMeetRoomName(settings, value);
					break;
				case Keys.Proctoring.JitsiMeet.SendAudio:
					MapJitsiMeetSendAudio(settings, value);
					break;
				case Keys.Proctoring.JitsiMeet.SendVideo:
					MapJitsiMeetSendVideo(settings, value);
					break;
				case Keys.Proctoring.JitsiMeet.ServerUrl:
					MapJitsiMeetServerUrl(settings, value);
					break;
				case Keys.Proctoring.JitsiMeet.ShowMeetingName:
					MapJitsiMeetShowMeetingName(settings, value);
					break;
				case Keys.Proctoring.JitsiMeet.Subject:
					MapJitsiMeetSubject(settings, value);
					break;
				case Keys.Proctoring.JitsiMeet.Token:
					MapJitsiMeetToken(settings, value);
					break;
				case Keys.Proctoring.JitsiMeet.VideoMuted:
					MapJitsiMeetVideoMuted(settings, value);
					break;
				case Keys.Proctoring.ShowRaiseHand:
					MapShowRaiseHand(settings, value);
					break;
				case Keys.Proctoring.ShowTaskbarNotification:
					MapShowTaskbarNotification(settings, value);
					break;
				case Keys.Proctoring.WindowVisibility:
					MapWindowVisibility(settings, value);
					break;
				case Keys.Proctoring.Zoom.AllowChat:
					MapZoomAllowChat(settings, value);
					break;
				case Keys.Proctoring.Zoom.AllowClosedCaptions:
					MapZoomAllowClosedCaptions(settings, value);
					break;
				case Keys.Proctoring.Zoom.AllowRaiseHand:
					MapZoomAllowRaiseHands(settings, value);
					break;
				case Keys.Proctoring.Zoom.AudioMuted:
					MapZoomAudioMuted(settings, value);
					break;
				case Keys.Proctoring.Zoom.Enabled:
					MapZoomEnabled(settings, value);
					break;
				case Keys.Proctoring.Zoom.MeetingNumber:
					MapZoomMeetingNumber(settings, value);
					break;
				case Keys.Proctoring.Zoom.ReceiveAudio:
					MapZoomReceiveAudio(settings, value);
					break;
				case Keys.Proctoring.Zoom.ReceiveVideo:
					MapZoomReceiveVideo(settings, value);
					break;
				case Keys.Proctoring.Zoom.SendAudio:
					MapZoomSendAudio(settings, value);
					break;
				case Keys.Proctoring.Zoom.SendVideo:
					MapZoomSendVideo(settings, value);
					break;
				case Keys.Proctoring.Zoom.Signature:
					MapZoomSignature(settings, value);
					break;
				case Keys.Proctoring.Zoom.Subject:
					MapZoomSubject(settings, value);
					break;
				case Keys.Proctoring.Zoom.UserName:
					MapZoomUserName(settings, value);
					break;
				case Keys.Proctoring.Zoom.VideoMuted:
					MapZoomVideoMuted(settings, value);
					break;
			}
		}

		private void MapForceRaiseHandMessage(AppSettings settings, object value)
		{
			if (value is bool force)
			{
				settings.Proctoring.ForceRaiseHandMessage = force;
			}
		}

		private void MapJitsiMeetAllowChat(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Proctoring.JitsiMeet.AllowChat = allow;
			}
		}

		private void MapJitsiMeetAllowClosedCaptions(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Proctoring.JitsiMeet.AllowClosedCaptions = allow;
			}
		}

		private void MapJitsiMeetAllowRaiseHands(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Proctoring.JitsiMeet.AllowRaiseHand = allow;
			}
		}

		private void MapJitsiMeetAllowRecording(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Proctoring.JitsiMeet.AllowRecording = allow;
			}
		}

		private void MapJitsiMeetAllowTileView(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Proctoring.JitsiMeet.AllowTileView = allow;
			}
		}

		private void MapJitsiMeetAudioMuted(AppSettings settings, object value)
		{
			if (value is bool audioMuted)
			{
				settings.Proctoring.JitsiMeet.AudioMuted = audioMuted;
			}
		}

		private void MapJitsiMeetAudioOnly(AppSettings settings, object value)
		{
			if (value is bool audioOnly)
			{
				settings.Proctoring.JitsiMeet.AudioOnly = audioOnly;
			}
		}

		private void MapJitsiMeetEnabled(AppSettings settings, object value)
		{
			if (value is bool enabled)
			{
				settings.Proctoring.JitsiMeet.Enabled = enabled;
			}
		}

		private void MapJitsiMeetReceiveAudio(AppSettings settings, object value)
		{
			if (value is bool receive)
			{
				settings.Proctoring.JitsiMeet.ReceiveAudio = receive;
			}
		}

		private void MapJitsiMeetReceiveVideo(AppSettings settings, object value)
		{
			if (value is bool receive)
			{
				settings.Proctoring.JitsiMeet.ReceiveVideo = receive;
			}
		}

		private void MapJitsiMeetRoomName(AppSettings settings, object value)
		{
			if (value is string name)
			{
				settings.Proctoring.JitsiMeet.RoomName = name;
			}
		}

		private void MapJitsiMeetSendAudio(AppSettings settings, object value)
		{
			if (value is bool send)
			{
				settings.Proctoring.JitsiMeet.SendAudio = send;
			}
		}

		private void MapJitsiMeetSendVideo(AppSettings settings, object value)
		{
			if (value is bool send)
			{
				settings.Proctoring.JitsiMeet.SendVideo = send;
			}
		}

		private void MapJitsiMeetServerUrl(AppSettings settings, object value)
		{
			if (value is string url)
			{
				settings.Proctoring.JitsiMeet.ServerUrl = url;
			}
		}

		private void MapJitsiMeetShowMeetingName(AppSettings settings, object value)
		{
			if (value is bool show)
			{
				settings.Proctoring.JitsiMeet.ShowMeetingName = show;
			}
		}

		private void MapJitsiMeetSubject(AppSettings settings, object value)
		{
			if (value is string subject)
			{
				settings.Proctoring.JitsiMeet.Subject = subject;
			}
		}

		private void MapJitsiMeetToken(AppSettings settings, object value)
		{
			if (value is string token)
			{
				settings.Proctoring.JitsiMeet.Token = token;
			}
		}

		private void MapJitsiMeetVideoMuted(AppSettings settings, object value)
		{
			if (value is bool muted)
			{
				settings.Proctoring.JitsiMeet.VideoMuted = muted;
			}
		}

		private void MapShowRaiseHand(AppSettings settings, object value)
		{
			if (value is bool show)
			{
				settings.Proctoring.ShowRaiseHandNotification = show;
			}
		}

		private void MapShowTaskbarNotification(AppSettings settings, object value)
		{
			if (value is bool show)
			{
				settings.Proctoring.ShowTaskbarNotification = show;
			}
		}

		private void MapWindowVisibility(AppSettings settings, object value)
		{
			const int HIDDEN = 0;
			const int ALLOW_SHOW = 1;
			const int ALLOW_HIDE = 2;
			const int VISIBLE = 3;

			if (value is int visibility)
			{
				switch (visibility)
				{
					case HIDDEN:
						settings.Proctoring.WindowVisibility = WindowVisibility.Hidden;
						break;
					case ALLOW_SHOW:
						settings.Proctoring.WindowVisibility = WindowVisibility.AllowToShow;
						break;
					case ALLOW_HIDE:
						settings.Proctoring.WindowVisibility = WindowVisibility.AllowToHide;
						break;
					case VISIBLE:
						settings.Proctoring.WindowVisibility = WindowVisibility.Visible;
						break;
				}
			}
		}

		private void MapZoomAllowChat(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Proctoring.Zoom.AllowChat = allow;
			}
		}

		private void MapZoomAllowClosedCaptions(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Proctoring.Zoom.AllowClosedCaptions = allow;
			}
		}

		private void MapZoomAllowRaiseHands(AppSettings settings, object value)
		{
			if (value is bool allow)
			{
				settings.Proctoring.Zoom.AllowRaiseHand = allow;
			}
		}

		private void MapZoomAudioMuted(AppSettings settings, object value)
		{
			if (value is bool muted)
			{
				settings.Proctoring.Zoom.AudioMuted = muted;
			}
		}

		private void MapZoomEnabled(AppSettings settings, object value)
		{
			if (value is bool enabled)
			{
				settings.Proctoring.Zoom.Enabled = enabled;
			}
		}

		private void MapZoomMeetingNumber(AppSettings settings, object value)
		{
			if (value is string number)
			{
				settings.Proctoring.Zoom.MeetingNumber = number;
			}
		}

		private void MapZoomReceiveAudio(AppSettings settings, object value)
		{
			if (value is bool receive)
			{
				settings.Proctoring.Zoom.ReceiveAudio = receive;
			}
		}

		private void MapZoomReceiveVideo(AppSettings settings, object value)
		{
			if (value is bool receive)
			{
				settings.Proctoring.Zoom.ReceiveVideo = receive;
			}
		}

		private void MapZoomSendAudio(AppSettings settings, object value)
		{
			if (value is bool send)
			{
				settings.Proctoring.Zoom.SendAudio = send;
			}
		}

		private void MapZoomSendVideo(AppSettings settings, object value)
		{
			if (value is bool send)
			{
				settings.Proctoring.Zoom.SendVideo = send;
			}
		}

		private void MapZoomSignature(AppSettings settings, object value)
		{
			if (value is string signature)
			{
				settings.Proctoring.Zoom.Signature = signature;
			}
		}

		private void MapZoomSubject(AppSettings settings, object value)
		{
			if (value is string subject)
			{
				settings.Proctoring.Zoom.Subject = subject;
			}
		}

		private void MapZoomUserName(AppSettings settings, object value)
		{
			if (value is string name)
			{
				settings.Proctoring.Zoom.UserName = name;
			}
		}

		private void MapZoomVideoMuted(AppSettings settings, object value)
		{
			if (value is bool muted)
			{
				settings.Proctoring.Zoom.VideoMuted = muted;
			}
		}
	}
}
