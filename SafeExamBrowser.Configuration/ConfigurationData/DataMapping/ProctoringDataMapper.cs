/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
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
				case Keys.Proctoring.JitsiMeet.RoomName:
					MapJitsiMeetRoomName(settings, value);
					break;
				case Keys.Proctoring.JitsiMeet.ServerUrl:
					MapJitsiMeetServerUrl(settings, value);
					break;
				case Keys.Proctoring.JitsiMeet.Subject:
					MapJitsiMeetSubject(settings, value);
					break;
				case Keys.Proctoring.JitsiMeet.Token:
					MapJitsiMeetToken(settings, value);
					break;
				case Keys.Proctoring.ShowTaskbarNotification:
					MapShowTaskbarNotification(settings, value);
					break;
				case Keys.Proctoring.WindowVisibility:
					MapWindowVisibility(settings, value);
					break;
			}
		}

		internal override void MapGlobal(IDictionary<string, object> rawData, AppSettings settings)
		{
			MapProctoringEnabled(rawData, settings);
		}

		private void MapProctoringEnabled(IDictionary<string, object> rawData, AppSettings settings)
		{
			var jitsiEnabled = rawData.TryGetValue(Keys.Proctoring.JitsiMeet.Enabled, out var v) && v is bool b && b;
			var zoomEnabled = rawData.TryGetValue(Keys.Proctoring.Zoom.Enabled, out v) && v is bool b2 && b2;

			settings.Proctoring.Enabled = jitsiEnabled || zoomEnabled;
		}

		private void MapJitsiMeetRoomName(AppSettings settings, object value)
		{
			if (value is string name)
			{
				settings.Proctoring.JitsiMeet.RoomName = name;
			}
		}

		private void MapJitsiMeetServerUrl(AppSettings settings, object value)
		{
			if (value is string url)
			{
				settings.Proctoring.JitsiMeet.ServerUrl = url;
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
	}
}
