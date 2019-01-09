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
			internal const string StartUrl = "startURL";
		}

		internal static class Input
		{
			internal static class Keyboard
			{
				public const string EnableAltEsc = "enableAltEsc";
				public const string EnableAltTab = "enableAltTab";
				public const string EnableAltF4 = "enableAltF4";
				public const string EnableCtrlEsc = "enableCtrlEsc";
				public const string EnableEsc = "enableEsc";
				public const string EnableF1 = "enableF1";
				public const string EnableF2 = "enableF2";
				public const string EnableF3 = "enableF3";
				public const string EnableF4 = "enableF4";
				public const string EnableF5 = "enableF5";
				public const string EnableF6 = "enableF6";
				public const string EnableF7 = "enableF7";
				public const string EnableF8 = "enableF8";
				public const string EnableF9 = "enableF9";
				public const string EnableF10 = "enableF10";
				public const string EnableF11 = "enableF11";
				public const string EnableF12 = "enableF12";
				public const string EnablePrintScreen = "enablePrintScreen";
				public const string EnableSystemKey = "enableStartMenu";
			}

			internal static class Mouse
			{
				public const string EnableRightMouse = "enableRightMouse";
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
		}
	}
}
