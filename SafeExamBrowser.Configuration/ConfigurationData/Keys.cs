/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Configuration.ConfigurationData
{
	internal static class Keys
	{
		internal static class AdditionalResources
		{
		}

		internal static class Applications
		{
		}

		internal static class Browser
		{
			internal const string AllowNavigation = "allowBrowsingBackForward";
			internal const string AllowPageZoom = "enableZoomPage";
			internal const string AllowPopups = "blockPopUpWindows";
			internal const string AllowReload = "browserWindowAllowReload";
			internal const string CustomUserAgentDesktop = "browserUserAgentWinDesktopModeCustom";
			internal const string CustomUserAgentMobile = "browserUserAgentWinTouchModeCustom";
			internal const string MainWindowMode = "browserViewMode";
			internal const string ShowReloadWarning = "showReloadWarning";
			internal const string UserAgentModeDesktop = "browserUserAgentWinDesktopMode";
			internal const string UserAgentModeMobile = "browserUserAgentWinTouchMode";
		}

		internal static class ConfigurationFile
		{
			internal const string ConfigurationPurpose = "sebConfigPurpose";
			internal const string KeepClientConfigEncryption = "clientConfigKeepEncryption";
		}

		internal static class Exam
		{
		}

		internal static class General
		{
			internal const string AdminPasswordHash = "hashedAdminPassword";
			internal const string LogLevel = "logLevel";
			internal const string QuitPasswordHash = "hashedQuitPassword";
			internal const string StartUrl = "startURL";
		}

		internal static class Input
		{
			internal static class Keyboard
			{
				internal const string EnableAltEsc = "enableAltEsc";
				internal const string EnableAltTab = "enableAltTab";
				internal const string EnableAltF4 = "enableAltF4";
				internal const string EnableCtrlEsc = "enableCtrlEsc";
				internal const string EnableEsc = "enableEsc";
				internal const string EnableF1 = "enableF1";
				internal const string EnableF2 = "enableF2";
				internal const string EnableF3 = "enableF3";
				internal const string EnableF4 = "enableF4";
				internal const string EnableF5 = "enableF5";
				internal const string EnableF6 = "enableF6";
				internal const string EnableF7 = "enableF7";
				internal const string EnableF8 = "enableF8";
				internal const string EnableF9 = "enableF9";
				internal const string EnableF10 = "enableF10";
				internal const string EnableF11 = "enableF11";
				internal const string EnableF12 = "enableF12";
				internal const string EnablePrintScreen = "enablePrintScreen";
				internal const string EnableSystemKey = "enableStartMenu";
			}

			internal static class Mouse
			{
				internal const string EnableRightMouse = "enableRightMouse";
			}
		}

		internal static class Network
		{
			internal static class Certificates
			{
				internal const string CertificateData = "certificateData";
				internal const string CertificateType = "type";
				internal const string EmbeddedCertificates = "embeddedCertificates";
			}
		}

		internal static class Registry
		{
		}

		internal static class Security
		{
		}

		internal static class UserInterface
		{
			internal const string AllowKeyboardLayout = "showInputLanguage";
			internal const string AllowLog = "showLogButton";
			internal const string AllowWirelessNetwork = "allowWlan";
			internal const string ShowClock = "showTime";
			internal const string UserInterfaceMode = "touchOptimized";
		}
	}
}
