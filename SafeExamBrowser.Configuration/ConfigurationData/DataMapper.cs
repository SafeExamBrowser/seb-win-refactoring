/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Settings;

namespace SafeExamBrowser.Configuration.ConfigurationData
{
	internal partial class DataMapper
	{
		internal void MapRawDataToSettings(IDictionary<string, object> rawData, AppSettings settings)
		{
			foreach (var item in rawData)
			{
				MapApplicationSettings(item.Key, item.Value, settings);
				MapAudioSettings(item.Key, item.Value, settings);
				MapBrowserSettings(item.Key, item.Value, settings);
				MapConfigurationFileSettings(item.Key, item.Value, settings);
				MapGeneralSettings(item.Key, item.Value, settings);
				MapKeyboardSettings(item.Key, item.Value, settings);
				MapMouseSettings(item.Key, item.Value, settings);
				MapSecuritySettings(item.Key, item.Value, settings);
				MapUserInterfaceSettings(item.Key, item.Value, settings);
			}

			MapApplicationLogAccess(rawData, settings);
			MapKioskMode(rawData, settings);
			MapPopupPolicy(rawData, settings);
			MapRequestFilter(rawData, settings);
			MapUserAgentMode(rawData, settings);
		}

		private void MapApplicationSettings(string key, object value, AppSettings settings)
		{
			switch (key)
			{
				case Keys.Applications.Blacklist:
					MapApplicationBlacklist(settings, value);
					break;
				case Keys.Applications.Whitelist:
					MapApplicationWhitelist(settings, value);
					break;
			}
		}

		private void MapAudioSettings(string key, object value, AppSettings settings)
		{
			switch (key)
			{
				case Keys.Audio.InitialVolumeLevel:
					MapInitialVolumeLevel(settings, value);
					break;
				case Keys.Audio.MuteAudio:
					MapMuteAudio(settings, value);
					break;
				case Keys.Audio.SetInitialVolumeLevel:
					MapSetInitialVolumeLevel(settings, value);
					break;
			}
		}

		private void MapBrowserSettings(string key, object value, AppSettings settings)
		{
			switch (key)
			{
				case Keys.Browser.AllowConfigurationDownloads:
					MapAllowConfigurationDownloads(settings, value);
					break;
				case Keys.Browser.AllowDeveloperConsole:
					MapAllowDeveloperConsole(settings, value);
					break;
				case Keys.Browser.AllowDownloads:
					MapAllowDownloads(settings, value);
					break;
				case Keys.Browser.AllowPageZoom:
					MapAllowPageZoom(settings, value);
					break;
				case Keys.Browser.AdditionalWindow.AllowAddressBar:
					MapAllowAddressBarAdditionalWindow(settings, value);
					break;
				case Keys.Browser.AdditionalWindow.AllowNavigation:
					MapAllowNavigationAdditionalWindow(settings, value);
					break;
				case Keys.Browser.AdditionalWindow.AllowReload:
					MapAllowReloadAdditionalWindow(settings, value);
					break;
				case Keys.Browser.AdditionalWindow.ShowReloadWarning:
					MapShowReloadWarningAdditionalWindow(settings, value);
					break;
				case Keys.Browser.AdditionalWindow.WindowHeight:
					MapWindowHeightAdditionalWindow(settings, value);
					break;
				case Keys.Browser.AdditionalWindow.WindowPosition:
					MapWindowPositionAdditionalWindow(settings, value);
					break;
				case Keys.Browser.AdditionalWindow.WindowWidth:
					MapWindowWidthAdditionalWindow(settings, value);
					break;
				case Keys.Browser.Filter.FilterRules:
					MapFilterRules(settings, value);
					break;
				case Keys.Browser.MainWindow.AllowAddressBar:
					MapAllowAddressBar(settings, value);
					break;
				case Keys.Browser.MainWindow.AllowNavigation:
					MapAllowNavigation(settings, value);
					break;
				case Keys.Browser.MainWindow.AllowReload:
					MapAllowReload(settings, value);
					break;
				case Keys.Browser.MainWindow.ShowReloadWarning:
					MapShowReloadWarning(settings, value);
					break;
				case Keys.Browser.MainWindow.WindowHeight:
					MapWindowHeightMainWindow(settings, value);
					break;
				case Keys.Browser.MainWindow.WindowMode:
					MapMainWindowMode(settings, value);
					break;
				case Keys.Browser.MainWindow.WindowPosition:
					MapWindowPositionMainWindow(settings, value);
					break;
				case Keys.Browser.MainWindow.WindowWidth:
					MapWindowWidthMainWindow(settings, value);
					break;
				case Keys.Browser.Proxy.Policy:
					MapProxyPolicy(settings, value);
					break;
				case Keys.Browser.Proxy.Settings:
					MapProxySettings(settings, value);
					break;
				case Keys.Browser.QuitUrl:
					MapQuitUrl(settings, value);
					break;
				case Keys.Browser.QuitUrlConfirmation:
					MapQuitUrlConfirmation(settings, value);
					break;
				case Keys.Browser.StartUrl:
					MapStartUrl(settings, value);
					break;
			}
		}

		private void MapConfigurationFileSettings(string key, object value, AppSettings settings)
		{
			switch (key)
			{
				case Keys.ConfigurationFile.AdminPasswordHash:
					MapAdminPasswordHash(settings, value);
					break;
				case Keys.ConfigurationFile.ConfigurationPurpose:
					MapConfigurationMode(settings, value);
					break;
			}
		}

		private void MapGeneralSettings(string key, object value, AppSettings settings)
		{
			switch (key)
			{
				case Keys.General.LogLevel:
					MapLogLevel(settings, value);
					break;
			}
		}

		private void MapKeyboardSettings(string key, object value, AppSettings settings)
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
			}
		}

		private void MapMouseSettings(string key, object value, AppSettings settings)
		{
			switch (key)
			{
				case Keys.Mouse.EnableRightMouse:
					MapEnableRightMouse(settings, value);
					break;
			}
		}

		private void MapSecuritySettings(string key, object value, AppSettings settings)
		{
			switch (key)
			{
				case Keys.Security.QuitPasswordHash:
					MapQuitPasswordHash(settings, value);
					break;
				case Keys.Security.ServicePolicy:
					MapServicePolicy(settings, value);
					break;
			}
		}

		private void MapUserInterfaceSettings(string key, object value, AppSettings settings)
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
	}
}
