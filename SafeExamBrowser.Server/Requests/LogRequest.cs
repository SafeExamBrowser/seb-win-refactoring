/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Net.Http;
using Newtonsoft.Json.Linq;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Server.Data;
using SafeExamBrowser.Settings.Server;

namespace SafeExamBrowser.Server.Requests
{
	internal class LogRequest : BaseRequest
	{
		internal LogRequest(
			ApiVersion1 api,
			HttpClient httpClient,
			ILogger logger,
			Parser parser,
			ServerSettings settings) : base(api, httpClient, logger, parser, settings)
		{
		}

		internal bool TryExecute(ILogMessage message)
		{
			var json = new JObject
			{
				["text"] = message.Message,
				["timestamp"] = message.DateTime.ToUnixTimestamp(),
				["type"] = message.Severity.ToLogType()
			};

			var success = TryExecute(HttpMethod.Post, api.LogEndpoint, out _, json.ToString(), ContentType.JSON, Authorization, Token);

			return success;
		}
	}
}
