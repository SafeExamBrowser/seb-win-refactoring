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
			if (value is bool allow)
			{
				settings.Taskbar.AllowApplicationLog = allow;
			}
		}

		private void MapClock(Settings settings, object value)
		{
			if (value is bool show)
			{
				settings.Taskbar.ShowClock = show;
			}
		}

		private void MapKeyboardLayout(Settings settings, object value)
		{
			if (value is bool enabled)
			{
				settings.Taskbar.AllowKeyboardLayout = enabled;
			}
		}

		private void MapWirelessNetwork(Settings settings, object value)
		{
			if (value is bool enabled)
			{
				settings.Taskbar.AllowWirelessNetwork = enabled;
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
