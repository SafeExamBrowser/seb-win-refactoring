/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Proctoring.ScreenProctoring.Service.Requests
{
	internal static class Header
	{
		internal const string ACCEPT = "Accept";
		internal const string AUTHORIZATION = "Authorization";
		internal const string GROUP_ID = "SEB_GROUP_UUID";
		internal const string HEALTH = "sps_server_health";
		internal const string IMAGE_FORMAT = "imageFormat";
		internal const string METADATA = "metaData";
		internal const string SESSION_ID = "SEB_SESSION_UUID";
		internal const string TIMESTAMP = "timestamp";

		internal static class Metadata
		{
			internal const string ApplicationInfo = "screenProctoringMetadataApplication";
			internal const string BrowserInfo = "screenProctoringMetadataBrowser";
			internal const string BrowserUrls = "screenProctoringMetadataURL";
			internal const string TriggerInfo = "screenProctoringMetadataUserAction";
			internal const string WindowTitle = "screenProctoringMetadataWindowTitle";
		}
	}
}
