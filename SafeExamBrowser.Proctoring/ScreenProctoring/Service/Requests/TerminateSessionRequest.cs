/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Net.Http;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Proctoring.ScreenProctoring.Service.Requests
{
	internal class TerminateSessionRequest : Request
	{
		internal TerminateSessionRequest(Api api, HttpClient httpClient, ILogger logger, Parser parser) : base(api, httpClient, logger, parser)
		{
		}

		internal bool TryExecute(string sessionId, out string message)
		{
			var url = $"{api.SessionEndpoint}/{sessionId}";
			var success = TryExecute(HttpMethod.Delete, url, out var response, contentType: ContentType.URL_ENCODED, headers: Authorization);

			message = response.ToLogString();

			return success;
		}
	}
}
