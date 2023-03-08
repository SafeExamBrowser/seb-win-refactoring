/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Server.Requests
{
	internal static class Header
	{
		internal const string ACCEPT = "Accept";
		internal const string APP_SIGNATURE_KEY_SALT = "SEBExamSalt";
		internal const string AUTHORIZATION = "Authorization";
		internal const string BROWSER_EXAM_KEY = "SEBServerBEK";
		internal const string CONNECTION_TOKEN = "SEBConnectionToken";
	}
}
