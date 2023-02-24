/*
 * Copyright (c) 2022 ETH Zürich, Educational Development and Technology (LET)
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
using SafeExamBrowser.Settings.Logging;
using SafeExamBrowser.Settings.Server;

namespace SafeExamBrowser.Server.Requests
{
	internal class PowerSupplyRequest : BaseRequest
	{
		internal PowerSupplyRequest(
			ApiVersion1 api,
			HttpClient httpClient,
			ILogger logger,
			Parser parser,
			ServerSettings settings) : base(api, httpClient, logger, parser, settings)
		{
		}

		internal bool TryExecute(string text, int value)
		{
			var json = new JObject
			{
				["numericValue"] = value,
				["text"] = text,
				["timestamp"] = DateTime.Now.ToUnixTimestamp(),
				["type"] = LogLevel.Info.ToLogType()
			};

			var success = TryExecute(HttpMethod.Post, api.LogEndpoint, out _, json.ToString(), ContentType.JSON, Authorization, Token);

			return success;
		}
	}
}
