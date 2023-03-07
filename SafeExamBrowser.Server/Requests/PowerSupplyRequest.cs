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
using SafeExamBrowser.SystemComponents.Contracts.PowerSupply;

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

		internal bool TryExecute(IPowerSupplyStatus status, bool previouslyConnected, int previousValue, out string message)
		{
			var connected = status.IsOnline;
			var text = default(string);
			var value = Convert.ToInt32(status.BatteryCharge * 100);

			if (value != previousValue)
			{
				var chargeInfo = $"{status.BatteryChargeStatus} at {value}%";
				var gridInfo = $"{(connected ? "connected to" : "disconnected from")} the power grid";

				text = $"<battery> {chargeInfo}, {status.BatteryTimeRemaining} remaining, {gridInfo}";
			}
			else if (connected != previouslyConnected)
			{
				text = $"<battery> Device has been {(connected ? "connected to" : "disconnected from")} power grid";
			}

			var json = new JObject
			{
				["numericValue"] = value,
				["text"] = text,
				["timestamp"] = DateTime.Now.ToUnixTimestamp(),
				["type"] = LogLevel.Info.ToLogType()
			};

			var success = TryExecute(HttpMethod.Post, api.LogEndpoint, out var response, json.ToString(), ContentType.JSON, Authorization, Token);

			message = response.ToLogString();

			return success;
		}
	}
}
