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
using SafeExamBrowser.Settings.Logging;
using SafeExamBrowser.Settings.Server;
using SafeExamBrowser.SystemComponents.Contracts.Network;

namespace SafeExamBrowser.Server.Requests
{
	internal class NetworkAdapterRequest : BaseRequest
	{
		internal NetworkAdapterRequest(
			ApiVersion1 api,
			HttpClient httpClient,
			ILogger logger,
			Parser parser,
			ServerSettings settings) : base(api, httpClient, logger, parser, settings)
		{
		}

		internal bool TryExecute(IWirelessNetwork network, out string message)
		{
			var json = new JObject
			{
				["text"] = $"<wlan> {(network != default ? $"{network.Name}: {network.Status}, {network.SignalStrength}%" : "not connected")}",
				["timestamp"] = DateTime.Now.ToUnixTimestamp(),
				["type"] = LogLevel.Info.ToLogType()
			};

			if (network != default)
			{
				json["numericValue"] = network.SignalStrength;
			}

			var success = TryExecute(HttpMethod.Post, api.LogEndpoint, out var response, json.ToString(), ContentType.JSON, Authorization, Token);

			message = response.ToLogString();

			return success;
		}
	}
}
