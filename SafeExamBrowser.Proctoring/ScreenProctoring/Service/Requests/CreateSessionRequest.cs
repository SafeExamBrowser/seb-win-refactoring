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
	internal class CreateSessionRequest : Request
	{
		internal CreateSessionRequest(Api api, HttpClient httpClient, ILogger logger, Parser parser) : base(api, httpClient, logger, parser)
		{
		}

		internal bool TryExecute(string groupId, out string message, out string sessionId)
		{
			var group = (Header.GROUP_ID, groupId);
			var success = TryExecute(HttpMethod.Post, api.SessionEndpoint, out var response, string.Empty, ContentType.URL_ENCODED, Authorization, group);

			message = response.ToLogString();
			sessionId = default;

			if (success)
			{
				parser.TryParseSessionId(response, out sessionId);
			}

			return success;
		}
	}
}
