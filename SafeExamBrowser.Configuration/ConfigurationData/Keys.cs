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
		internal static class General
		{
			internal const string AdminPasswordHash = "hashedAdminPassword";
			internal const string StartUrl = "startURL";
		}

		internal static class ConfigurationFile
		{
			internal const string ConfigurationPurpose = "sebConfigPurpose";
			internal const string KeepClientConfigEncryption = "clientConfigKeepEncryption";
		}

		internal static class UserInterface
		{
		}

		internal static class Browser
		{
		}

		internal static class DownUploads
		{
		}

		internal static class Exam
		{
		}

		internal static class Applications
		{
		}

		internal static class AdditionalResources
		{
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

		internal static class Security
		{
		}

		internal static class Registry
		{
		}

		internal static class HookedKeys
		{
		}
	}
}
