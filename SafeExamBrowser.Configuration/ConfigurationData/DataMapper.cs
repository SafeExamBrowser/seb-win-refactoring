/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Contracts.Configuration.Settings;

namespace SafeExamBrowser.Configuration.ConfigurationData
{
	internal partial class DataMapper
	{
		internal void MapRawDataToSettings(IDictionary<string, object> rawData, Settings settings)
		{
			foreach (var item in rawData)
			{
				Map(item.Key, item.Value, settings);
			}

			MapUserAgentMode(rawData, settings);
		}

		private void Map(string key, object value, Settings settings)
		{
			switch (key)
			{
				case Keys.Browser.AllowNavigation:
					MapAllowNavigation(settings, value);
					break;
				case Keys.Browser.AllowNavigationAdditionalWindow:
					MapAllowNavigationAdditionalWindow(settings, value);
					break;
				case Keys.Browser.AllowPageZoom:
					MapAllowPageZoom(settings, value);
					break;
				case Keys.Browser.AllowPopups:
					MapAllowPopups(settings, value);
					break;
				case Keys.Browser.AllowReload:
					MapAllowReload(settings, value);
					break;
				case Keys.Browser.AllowReloadAdditionalWindow:
					MapAllowReloadAdditionalWindow(settings, value);
					break;
				case Keys.Browser.MainWindowMode:
					MapMainWindowMode(settings, value);
					break;
				case Keys.Browser.ShowReloadWarning:
					MapShowReloadWarning(settings, value);
					break;
				case Keys.Browser.ShowReloadWarningAdditionalWindow:
					MapShowReloadWarningAdditionalWindow(settings, value);
					break;
				case Keys.ConfigurationFile.ConfigurationPurpose:
					MapConfigurationMode(settings, value);
					break;
				case Keys.General.AdminPasswordHash:
					MapAdminPasswordHash(settings, value);
					break;
				case Keys.General.LogLevel:
					MapLogLevel(settings, value);
					break;
				case Keys.General.QuitPasswordHash:
					MapQuitPasswordHash(settings, value);
					break;
				case Keys.General.StartUrl:
					MapStartUrl(settings, value);
					break;
				case Keys.Input.Keyboard.EnableAltEsc:
					MapEnableAltEsc(settings, value);
					break;
				case Keys.Input.Keyboard.EnableAltF4:
					MapEnableAltF4(settings, value);
					break;
				case Keys.Input.Keyboard.EnableAltTab:
					MapEnableAltTab(settings, value);
					break;
				case Keys.Input.Keyboard.EnableCtrlEsc:
					MapEnableCtrlEsc(settings, value);
					break;
				case Keys.Input.Keyboard.EnableEsc:
					MapEnableEsc(settings, value);
					break;
				case Keys.Input.Keyboard.EnableF1:
					MapEnableF1(settings, value);
					break;
				case Keys.Input.Keyboard.EnableF2:
					MapEnableF2(settings, value);
					break;
				case Keys.Input.Keyboard.EnableF3:
					MapEnableF3(settings, value);
					break;
				case Keys.Input.Keyboard.EnableF4:
					MapEnableF4(settings, value);
					break;
				case Keys.Input.Keyboard.EnableF5:
					MapEnableF5(settings, value);
					break;
				case Keys.Input.Keyboard.EnableF6:
					MapEnableF6(settings, value);
					break;
				case Keys.Input.Keyboard.EnableF7:
					MapEnableF7(settings, value);
					break;
				case Keys.Input.Keyboard.EnableF8:
					MapEnableF8(settings, value);
					break;
				case Keys.Input.Keyboard.EnableF9:
					MapEnableF9(settings, value);
					break;
				case Keys.Input.Keyboard.EnableF10:
					MapEnableF10(settings, value);
					break;
				case Keys.Input.Keyboard.EnableF11:
					MapEnableF11(settings, value);
					break;
				case Keys.Input.Keyboard.EnableF12:
					MapEnableF12(settings, value);
					break;
				case Keys.Input.Keyboard.EnablePrintScreen:
					MapEnablePrintScreen(settings, value);
					break;
				case Keys.Input.Keyboard.EnableSystemKey:
					MapEnableSystemKey(settings, value);
					break;
				case Keys.Input.Mouse.EnableRightMouse:
					MapEnableRightMouse(settings, value);
					break;
				case Keys.UserInterface.AllowKeyboardLayout:
					MapKeyboardLayout(settings, value);
					break;
				case Keys.UserInterface.AllowLog:
					MapApplicationLog(settings, value);
					break;
				case Keys.UserInterface.AllowWirelessNetwork:
					MapWirelessNetwork(settings, value);
					break;
				case Keys.UserInterface.ShowClock:
					MapClock(settings, value);
					break;
				case Keys.UserInterface.UserInterfaceMode:
					MapUserInterfaceMode(settings, value);
					break;
			}
		}
	}
}
