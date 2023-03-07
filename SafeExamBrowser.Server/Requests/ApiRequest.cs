/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Net.Http;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Server.Data;
using SafeExamBrowser.Settings.Server;

namespace SafeExamBrowser.Server.Requests
{
	internal class ApiRequest : BaseRequest
	{
		internal ApiRequest(
			ApiVersion1 api,
			HttpClient httpClient,
			ILogger logger,
			Parser parser,
			ServerSettings settings) : base(api, httpClient, logger, parser, settings)
		{
		}

		internal bool TryExecute(out ApiVersion1 api, out string message)
		{
			var success = TryExecute(HttpMethod.Get, settings.ApiUrl, out var response);

			api = new ApiVersion1();
			message = response.ToLogString();

			if (success)
			{
				parser.TryParseApi(response.Content, out api);
			}

			return success;
		}
	}
}
