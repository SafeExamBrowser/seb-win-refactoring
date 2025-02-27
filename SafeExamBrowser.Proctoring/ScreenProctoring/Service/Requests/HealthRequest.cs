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
	internal class HealthRequest : Request
	{
		internal HealthRequest(Api api, HttpClient httpClient, ILogger logger, Parser parser) : base(api, httpClient, logger, parser)
		{
		}

		internal bool TryExecute(out int health, out string message)
		{
			var url = api.HealthEndpoint;
			var success = TryExecute(HttpMethod.Get, url, out var response);

			health = default;
			message = response.ToLogString();

			if (success)
			{
				parser.TryParseHealth(response, out health);
			}

			return success;
		}
	}
}
