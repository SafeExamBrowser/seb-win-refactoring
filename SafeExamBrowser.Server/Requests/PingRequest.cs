/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Net.Http;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Server.Data;
using SafeExamBrowser.Settings.Server;

namespace SafeExamBrowser.Server.Requests
{
	internal class PingRequest : BaseRequest
	{
		internal PingRequest(
			ApiVersion1 api,
			HttpClient httpClient,
			ILogger logger,
			Parser parser,
			ServerSettings settings) : base(api, httpClient, logger, parser, settings)
		{
		}

		internal bool TryExecute(int pingNumber, out HttpContent content, out string message, string confirmation = default)
		{
			var requestContent = $"timestamp={DateTime.Now.ToUnixTimestamp()}&ping-number={pingNumber}";

			if (confirmation != default)
			{
				requestContent = $"{requestContent}&instruction-confirm={confirmation}";
			}

			var success = TryExecute(HttpMethod.Post, api.PingEndpoint, out var response, requestContent, ContentType.URL_ENCODED, Authorization, Token);

			content = response.Content;
			message = response.ToLogString();

			return success;
		}
	}
}
