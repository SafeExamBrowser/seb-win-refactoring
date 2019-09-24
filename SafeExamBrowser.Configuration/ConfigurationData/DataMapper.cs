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
		internal void MapRawDataToSettings(IDictionary<string, object> rawData, ApplicationSettings settings)
		{
			foreach (var item in rawData)
			{
				MapAudioSettings(item.Key, item.Value, settings);
				MapBrowserSettings(item.Key, item.Value, settings);
				MapConfigurationFileSettings(item.Key, item.Value, settings);
				MapGeneralSettings(item.Key, item.Value, settings);
				MapInputSettings(item.Key, item.Value, settings);
				MapSecuritySettings(item.Key, item.Value, settings);
				MapUserInterfaceSettings(item.Key, item.Value, settings);
			}

			MapApplicationLogAccess(rawData, settings);
			MapKioskMode(rawData, settings);
			MapUserAgentMode(rawData, settings);
		}

		private void MapAudioSettings(string key, object value, ApplicationSettings settings)
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

		private void MapBrowserSettings(string key, object value, ApplicationSettings settings)
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
				case Keys.Browser.AllowPopups:
					MapAllowPopups(settings, value);
					break;
				case Keys.Browser.MainWindowMode:
					MapMainWindowMode(settings, value);
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
				case Keys.Browser.Filter.EnableContentRequestFilter:
					MapEnableContentRequestFilter(settings, value);
					break;
				case Keys.Browser.Filter.EnableMainRequestFilter:
					MapEnableMainRequestFilter(settings, value);
					break;
				case Keys.Browser.Filter.UrlFilterRules:
					MapUrlFilterRules(settings, value);
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
			}
		}

		private void MapConfigurationFileSettings(string key, object value, ApplicationSettings settings)
		{
			switch (key)
			{
				case Keys.ConfigurationFile.ConfigurationPurpose:
					MapConfigurationMode(settings, value);
					break;
			}
		}

		private void MapGeneralSettings(string key, object value, ApplicationSettings settings)
		{
			switch (key)
			{
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
			}
		}

		private void MapInputSettings(string key, object value, ApplicationSettings settings)
		{
			switch (key)
			{
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
			}
		}

		private void MapSecuritySettings(string key, object value, ApplicationSettings settings)
		{
			switch (key)
			{
				case Keys.Security.ServicePolicy:
					MapServicePolicy(settings, value);
					break;
			}
		}

		private void MapUserInterfaceSettings(string key, object value, ApplicationSettings settings)
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
