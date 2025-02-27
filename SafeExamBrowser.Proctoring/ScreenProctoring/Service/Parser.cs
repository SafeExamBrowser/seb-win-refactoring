/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Proctoring.ScreenProctoring.Service.Requests;

namespace SafeExamBrowser.Proctoring.ScreenProctoring.Service
{
	internal class Parser
	{
		private readonly ILogger logger;

		internal Parser(ILogger logger)
		{
			this.logger = logger;
		}

		internal bool IsTokenExpired(HttpContent content)
		{
			var isExpired = false;

			try
			{
				var json = JsonConvert.DeserializeObject(Extract(content)) as JObject;
				var error = json["error"].Value<string>();

				isExpired = error?.Equals("invalid_token", StringComparison.OrdinalIgnoreCase) == true;
			}
			catch (Exception e)
			{
				logger.Error("Failed to parse token expiration content!", e);
			}

			return isExpired;
		}

		internal bool TryParseHealth(HttpResponseMessage response, out int health)
		{
			var success = false;

			health = default;

			try
			{
				if (response.Headers.TryGetValues(Header.HEALTH, out var values))
				{
					success = int.TryParse(values.First(), out health);
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to parse health!", e);
			}

			return success;
		}

		internal bool TryParseOauth2Token(HttpContent content, out string oauth2Token)
		{
			oauth2Token = default;

			try
			{
				var json = JsonConvert.DeserializeObject(Extract(content)) as JObject;

				oauth2Token = json["access_token"].Value<string>();
			}
			catch (Exception e)
			{
				logger.Error("Failed to parse Oauth2 token!", e);
			}

			return oauth2Token != default;
		}

		internal bool TryParseSessionId(HttpResponseMessage response, out string sessionId)
		{
			sessionId = default;

			try
			{
				if (response.Headers.TryGetValues(Header.SESSION_ID, out var values))
				{
					sessionId = values.First();
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to parse session identifier!", e);
			}

			return sessionId != default;
		}

		private string Extract(HttpContent content)
		{
			var task = Task.Run(async () =>
			{
				return await content.ReadAsStreamAsync();
			});
			var stream = task.GetAwaiter().GetResult();
			var reader = new StreamReader(stream);

			return reader.ReadToEnd();
		}
	}
}
