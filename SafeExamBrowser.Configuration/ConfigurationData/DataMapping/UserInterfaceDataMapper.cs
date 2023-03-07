/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.UserInterface;

namespace SafeExamBrowser.Configuration.ConfigurationData.DataMapping
{
	internal class UserInterfaceDataMapper : BaseDataMapper
	{
		internal override void Map(string key, object value, AppSettings settings)
		{
			switch (key)
			{
				case Keys.UserInterface.ActionCenter.EnableActionCenter:
					MapEnableActionCenter(settings, value);
					break;
				case Keys.UserInterface.ShowAudio:
					MapShowAudio(settings, value);
					break;
				case Keys.UserInterface.ShowClock:
					MapShowClock(settings, value);
					break;
				case Keys.UserInterface.ShowKeyboardLayout:
					MapShowKeyboardLayout(settings, value);
					break;
				case Keys.UserInterface.ShowNetwork:
					MapShowNetwork(settings, value);
					break;
				case Keys.UserInterface.Taskbar.EnableTaskbar:
					MapEnableTaskbar(settings, value);
					break;
				case Keys.UserInterface.Taskbar.ShowApplicationLog:
					MapShowApplicationLog(settings, value);
					break;
				case Keys.UserInterface.UserInterfaceMode:
					MapUserInterfaceMode(settings, value);
					break;
			}
		}

		private void MapEnableActionCenter(AppSettings settings, object value)
		{
			if (value is bool enable)
			{
				settings.ActionCenter.EnableActionCenter = enable;
			}
		}

		private void MapShowAudio(AppSettings settings, object value)
		{
			if (value is bool show)
			{
				settings.ActionCenter.ShowAudio = show;
				settings.Taskbar.ShowAudio = show;
			}
		}

		private void MapShowClock(AppSettings settings, object value)
		{
			if (value is bool show)
			{
				settings.ActionCenter.ShowClock = show;
				settings.Taskbar.ShowClock = show;
			}
		}

		private void MapShowKeyboardLayout(AppSettings settings, object value)
		{
			if (value is bool show)
			{
				settings.ActionCenter.ShowKeyboardLayout = show;
				settings.Taskbar.ShowKeyboardLayout = show;
			}
		}

		private void MapShowNetwork(AppSettings settings, object value)
		{
			if (value is bool show)
			{
				settings.ActionCenter.ShowNetwork = show;
				settings.Taskbar.ShowNetwork = show;
			}
		}

		private void MapEnableTaskbar(AppSettings settings, object value)
		{
			if (value is bool enable)
			{
				settings.Taskbar.EnableTaskbar = enable;
			}
		}

		private void MapShowApplicationLog(AppSettings settings, object value)
		{
			if (value is bool show)
			{
				settings.Taskbar.ShowApplicationLog = show;
			}
		}

		private void MapUserInterfaceMode(AppSettings settings, object value)
		{
			if (value is bool mobile)
			{
				settings.UserInterfaceMode = mobile ? UserInterfaceMode.Mobile : UserInterfaceMode.Desktop;
			}
		}
	}
}
