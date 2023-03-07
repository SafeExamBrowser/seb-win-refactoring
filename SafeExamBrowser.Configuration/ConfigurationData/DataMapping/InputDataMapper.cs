/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Settings;

namespace SafeExamBrowser.Configuration.ConfigurationData.DataMapping
{
	internal class InputDataMapper : BaseDataMapper
	{
		internal override void Map(string key, object value, AppSettings settings)
		{
			switch (key)
			{
				case Keys.Keyboard.EnableAltEsc:
					MapEnableAltEsc(settings, value);
					break;
				case Keys.Keyboard.EnableAltF4:
					MapEnableAltF4(settings, value);
					break;
				case Keys.Keyboard.EnableAltTab:
					MapEnableAltTab(settings, value);
					break;
				case Keys.Keyboard.EnableCtrlEsc:
					MapEnableCtrlEsc(settings, value);
					break;
				case Keys.Keyboard.EnableEsc:
					MapEnableEsc(settings, value);
					break;
				case Keys.Keyboard.EnableF1:
					MapEnableF1(settings, value);
					break;
				case Keys.Keyboard.EnableF2:
					MapEnableF2(settings, value);
					break;
				case Keys.Keyboard.EnableF3:
					MapEnableF3(settings, value);
					break;
				case Keys.Keyboard.EnableF4:
					MapEnableF4(settings, value);
					break;
				case Keys.Keyboard.EnableF5:
					MapEnableF5(settings, value);
					break;
				case Keys.Keyboard.EnableF6:
					MapEnableF6(settings, value);
					break;
				case Keys.Keyboard.EnableF7:
					MapEnableF7(settings, value);
					break;
				case Keys.Keyboard.EnableF8:
					MapEnableF8(settings, value);
					break;
				case Keys.Keyboard.EnableF9:
					MapEnableF9(settings, value);
					break;
				case Keys.Keyboard.EnableF10:
					MapEnableF10(settings, value);
					break;
				case Keys.Keyboard.EnableF11:
					MapEnableF11(settings, value);
					break;
				case Keys.Keyboard.EnableF12:
					MapEnableF12(settings, value);
					break;
				case Keys.Keyboard.EnablePrintScreen:
					MapEnablePrintScreen(settings, value);
					break;
				case Keys.Keyboard.EnableSystemKey:
					MapEnableSystemKey(settings, value);
					break;
				case Keys.Mouse.EnableMiddleMouseButton:
					MapEnableMiddleMouseButton(settings, value);
					break;
				case Keys.Mouse.EnableRightMouseButton:
					MapEnableRightMouseButton(settings, value);
					break;
			}
		}

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

		private void MapEnableMiddleMouseButton(AppSettings settings, object value)
		{
			if (value is bool enabled)
			{
				settings.Mouse.AllowMiddleButton = enabled;
			}
		}

		private void MapEnableRightMouseButton(AppSettings settings, object value)
		{
			if (value is bool enabled)
			{
				settings.Mouse.AllowRightButton = enabled;
			}
		}
	}
}
