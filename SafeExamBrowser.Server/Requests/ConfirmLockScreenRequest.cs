/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Server.Data;
using SafeExamBrowser.Settings.Server;

namespace SafeExamBrowser.Server.Requests
{
	internal class ConfirmLockScreenRequest : BaseRequest
	{
		internal ConfirmLockScreenRequest(
			ApiVersion1 api,
			HttpClient httpClient,
			ILogger logger,
			Parser parser,
			ServerSettings settings) : base(api, httpClient, logger, parser, settings)
		{
		}

		internal bool TryExecute(int lockScreenId, out string message)
		{
			var json = new JObject
			{
				["numericValue"] = lockScreenId,
				["timestamp"] = DateTime.Now.ToUnixTimestamp(),
				["type"] = "NOTIFICATION_CONFIRMED"
			};
			var content = json.ToString();
			var success = TryExecute(HttpMethod.Post, api.LogEndpoint, out var response, content, ContentType.JSON, Authorization, Token);

			message = response.ToLogString();

			return success;
		}
	}
}
