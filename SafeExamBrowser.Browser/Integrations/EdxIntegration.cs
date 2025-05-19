/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using CefSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Browser.Integrations
{
	internal class EdxIntegration : Integration
	{
		private readonly ILogger logger;

		public EdxIntegration(ILogger logger)
		{
			this.logger = logger;
		}

		internal override bool TrySearchUserIdentifier(Cookie cookie, out string userIdentifier)
		{
			userIdentifier = default;

			return false;
		}

		internal override bool TrySearchUserIdentifier(IRequest request, IResponse response, out string userIdentifier)
		{
			var cookies = response.Headers.GetValues("Set-Cookie");
			var userInfo = cookies?.FirstOrDefault(c => c.Contains("edx-user-info"));

			userIdentifier = default;

			if (TryParseCookie(userInfo, out var id) && HasChanged(id))
			{
				userIdentifier = id;
				logger.Info("User identifier detected by session cookie on response.");
			}

			return userIdentifier != default;
		}

		private bool TryParseCookie(string userInfo, out string userIdentifier)
		{
			userIdentifier = default;

			try
			{
				if (userInfo != default)
				{
					var start = userInfo.IndexOf("=") + 1;
					var end = userInfo.IndexOf("; expires");
					var cookie = userInfo.Substring(start, end - start);
					var sanitized = cookie.Replace("\\\"", "\"").Replace("\\054", ",").Trim('"');
					var json = JsonConvert.DeserializeObject(sanitized) as JObject;

					userIdentifier = json["username"].Value<string>();
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to parse user identifier!", e);
			}

			return userIdentifier != default;
		}
	}
}
