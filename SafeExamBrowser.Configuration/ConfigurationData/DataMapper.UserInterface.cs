/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Settings;
using SafeExamBrowser.Settings.UserInterface;

namespace SafeExamBrowser.Configuration.ConfigurationData
{
	internal partial class DataMapper
	{
		private void MapApplicationLogButton(ApplicationSettings settings, object value)
		{
			if (value is bool show)
			{
				settings.Taskbar.ShowApplicationLog = show;
			}
		}

		private void MapAudio(ApplicationSettings settings, object value)
		{
			if (value is bool show)
			{
				settings.ActionCenter.ShowAudio = show;
				settings.Taskbar.ShowAudio = show;
			}
		}

		private void MapClock(ApplicationSettings settings, object value)
		{
			if (value is bool show)
			{
				settings.ActionCenter.ShowClock = show;
				settings.Taskbar.ShowClock = show;
			}
		}

		private void MapKeyboardLayout(ApplicationSettings settings, object value)
		{
			if (value is bool show)
			{
				settings.ActionCenter.ShowKeyboardLayout = show;
				settings.Taskbar.ShowKeyboardLayout = show;
			}
		}

		private void MapWirelessNetwork(ApplicationSettings settings, object value)
		{
			if (value is bool show)
			{
				settings.ActionCenter.ShowWirelessNetwork = show;
				settings.Taskbar.ShowWirelessNetwork = show;
			}
		}

		private void MapUserInterfaceMode(ApplicationSettings settings, object value)
		{
			if (value is bool mobile)
			{
				settings.UserInterfaceMode = mobile ? UserInterfaceMode.Mobile : UserInterfaceMode.Desktop;
			}
		}
	}
}
