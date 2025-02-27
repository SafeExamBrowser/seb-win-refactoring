/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Proctoring.ScreenProctoring.Service
{
	internal class Api
	{
		internal const string SESSION_ID = "%%_SESSION_ID_%%";

		internal string AccessTokenEndpoint { get; set; }
		internal string HealthEndpoint { get; set; }
		internal string ScreenShotEndpoint { get; set; }
		internal string SessionEndpoint { get; set; }

		internal Api()
		{
			AccessTokenEndpoint = "oauth/token";
			HealthEndpoint = "health";
			ScreenShotEndpoint = $"seb-api/v1/session/{SESSION_ID}/screenshot";
			SessionEndpoint = "seb-api/v1/session";
		}
	}
}
