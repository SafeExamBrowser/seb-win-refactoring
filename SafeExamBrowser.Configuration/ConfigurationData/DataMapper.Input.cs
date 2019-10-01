/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Settings;

namespace SafeExamBrowser.Configuration.ConfigurationData
{
	internal partial class DataMapper
	{
		private void MapEnableAltEsc(AppSettings settings, object value)
		{
			if (value is bool enabled)
			{
				settings.Keyboard.AllowAltEsc = enabled;
			}
		}

		private void MapEnableAltF4(AppSettings settings, object value)
		{
			if (value is bool enabled)
			{
				settings.Keyboard.AllowAltF4 = enabled;
			}
		}

		private void MapEnableAltTab(AppSettings settings, object value)
		{
			if (value is bool enabled)
			{
				settings.Keyboard.AllowAltTab = enabled;
			}
		}

		private void MapEnableCtrlEsc(AppSettings settings, object value)
		{
			if (value is bool enabled)
			{
				settings.Keyboard.AllowCtrlEsc = enabled;
			}
		}

		private void MapEnableEsc(AppSettings settings, object value)
		{
			if (value is bool enabled)
			{
				settings.Keyboard.AllowEsc = enabled;
			}
		}

		private void MapEnableF1(AppSettings settings, object value)
		{
			if (value is bool enabled)
			{
				settings.Keyboard.AllowF1 = enabled;
			}
		}

		private void MapEnableF2(AppSettings settings, object value)
		{
			if (value is bool enabled)
			{
				settings.Keyboard.AllowF2 = enabled;
			}
		}

		private void MapEnableF3(AppSettings settings, object value)
		{
			if (value is bool enabled)
			{
				settings.Keyboard.AllowF3 = enabled;
			}
		}

		private void MapEnableF4(AppSettings settings, object value)
		{
			if (value is bool enabled)
			{
				settings.Keyboard.AllowF4 = enabled;
			}
		}

		private void MapEnableF5(AppSettings settings, object value)
		{
			if (value is bool enabled)
			{
				settings.Keyboard.AllowF5 = enabled;
			}
		}

		private void MapEnableF6(AppSettings settings, object value)
		{
			if (value is bool enabled)
			{
				settings.Keyboard.AllowF6 = enabled;
			}
		}

		private void MapEnableF7(AppSettings settings, object value)
		{
			if (value is bool enabled)
			{
				settings.Keyboard.AllowF7 = enabled;
			}
		}

		private void MapEnableF8(AppSettings settings, object value)
		{
			if (value is bool enabled)
			{
				settings.Keyboard.AllowF8 = enabled;
			}
		}

		private void MapEnableF9(AppSettings settings, object value)
		{
			if (value is bool enabled)
			{
				settings.Keyboard.AllowF9 = enabled;
			}
		}

		private void MapEnableF10(AppSettings settings, object value)
		{
			if (value is bool enabled)
			{
				settings.Keyboard.AllowF10 = enabled;
			}
		}

		private void MapEnableF11(AppSettings settings, object value)
		{
			if (value is bool enabled)
			{
				settings.Keyboard.AllowF11 = enabled;
			}
		}

		private void MapEnableF12(AppSettings settings, object value)
		{
			if (value is bool enabled)
			{
				settings.Keyboard.AllowF12 = enabled;
			}
		}

		private void MapEnablePrintScreen(AppSettings settings, object value)
		{
			if (value is bool enabled)
			{
				settings.Keyboard.AllowPrintScreen = enabled;
			}
		}

		private void MapEnableSystemKey(AppSettings settings, object value)
		{
			if (value is bool enabled)
			{
				settings.Keyboard.AllowSystemKey = enabled;
			}
		}

		private void MapEnableRightMouse(AppSettings settings, object value)
		{
			if (value is bool enabled)
			{
				settings.Mouse.AllowRightButton = enabled;
			}
		}
	}
}
