/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
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
				case Keys.UserInterface.ShowAudio:
					MapAudio(settings, value);
					break;
				case Keys.UserInterface.ShowKeyboardLayout:
					MapKeyboardLayout(settings, value);
					break;
				case Keys.UserInterface.ShowWirelessNetwork:
					MapWirelessNetwork(settings, value);
					break;
				case Keys.UserInterface.ShowClock:
					MapClock(settings, value);
					break;
				case Keys.UserInterface.UserInterfaceMode:
					MapUserInterfaceMode(settings, value);
					break;
				case Keys.UserInterface.Taskbar.ShowApplicationLog:
					MapApplicationLogButton(settings, value);
					break;
			}
		}

		private void MapApplicationLogButton(AppSettings settings, object value)
		{
			if (value is bool show)
			{
				settings.Taskbar.ShowApplicationLog = show;
			}
		}

		private void MapAudio(AppSettings settings, object value)
		{
			if (value is bool show)
			{
				settings.ActionCenter.ShowAudio = show;
				settings.Taskbar.ShowAudio = show;
			}
		}

		private void MapClock(AppSettings settings, object value)
		{
			if (value is bool show)
			{
				settings.ActionCenter.ShowClock = show;
				settings.Taskbar.ShowClock = show;
			}
		}

		private void MapKeyboardLayout(AppSettings settings, object value)
		{
			if (value is bool show)
			{
				settings.ActionCenter.ShowKeyboardLayout = show;
				settings.Taskbar.ShowKeyboardLayout = show;
			}
		}

		private void MapWirelessNetwork(AppSettings settings, object value)
		{
			if (value is bool show)
			{
				settings.ActionCenter.ShowWirelessNetwork = show;
				settings.Taskbar.ShowWirelessNetwork = show;
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
