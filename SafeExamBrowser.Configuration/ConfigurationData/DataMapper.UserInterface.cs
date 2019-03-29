/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Configuration.Settings;

namespace SafeExamBrowser.Configuration.ConfigurationData
{
	internal partial class DataMapper
	{
		private void MapApplicationLog(Settings settings, object value)
		{
			if (value is bool show)
			{
				settings.ActionCenter.ShowApplicationLog = show;
				settings.Taskbar.ShowApplicationLog = show;
			}
		}

		private void MapClock(Settings settings, object value)
		{
			if (value is bool show)
			{
				settings.ActionCenter.ShowClock = show;
				settings.Taskbar.ShowClock = show;
			}
		}

		private void MapKeyboardLayout(Settings settings, object value)
		{
			if (value is bool show)
			{
				settings.ActionCenter.ShowKeyboardLayout = show;
				settings.Taskbar.ShowKeyboardLayout = show;
			}
		}

		private void MapWirelessNetwork(Settings settings, object value)
		{
			if (value is bool show)
			{
				settings.ActionCenter.ShowWirelessNetwork = show;
				settings.Taskbar.ShowWirelessNetwork = show;
			}
		}

		private void MapUserInterfaceMode(Settings settings, object value)
		{
			if (value is bool mobile)
			{
				settings.UserInterfaceMode = mobile ? UserInterfaceMode.Mobile : UserInterfaceMode.Desktop;
			}
		}
	}
}
