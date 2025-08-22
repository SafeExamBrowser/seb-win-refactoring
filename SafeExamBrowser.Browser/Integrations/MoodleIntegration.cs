/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using CefSharp;
using SafeExamBrowser.Logging.Contracts;
using Cookie = CefSharp.Cookie;

namespace SafeExamBrowser.Browser.Integrations
{
	internal class MoodleIntegration : Integration
	{
		private const string PLUGIN_PATH = "/mod/quiz/accessrule/sebserver/classes/external/user.php";
		private const string SESSION_COOKIE_NAME = "MoodleSession";
		private const string THEME_PATH = "/theme/boost_ethz/sebuser.php";

		private readonly ILogger logger;

		public MoodleIntegration(ILogger logger)
		{
			this.logger = logger;
		}

		internal override bool TrySearchUserIdentifier(Cookie cookie, out string userIdentifier)
		{
			return TrySearchByCookie(cookie, out userIdentifier);
		}

		internal override bool TrySearchUserIdentifier(IRequest request, IResponse response, out string userIdentifier)
		{
			var success = TrySearchByLocation(response, out userIdentifier);

			if (!success)
			{
				success = TrySearchByRequests(request, response, out userIdentifier);
			}

			return success;
		}

		private bool TrySearchByCookie(Cookie cookie, out string userIdentifier)
		{
			var id = default(string);
			var type = default(RequestType);
			var isSession = cookie.Name.Contains(SESSION_COOKIE_NAME);
			var url = $"{(cookie.Secure ? Uri.UriSchemeHttps : Uri.UriSchemeHttp)}{Uri.SchemeDelimiter}{cookie.Domain}{cookie.Path}";
			var hasId = isSession && TryExecuteRequests(url, (cookie.Name, cookie.Value), out type, out id);

			userIdentifier = default;

			if (hasId && HasChanged(id))
			{
				userIdentifier = id;
				logger.Info($"User identifier '{id}' detected by request on cookie traversal ({type}).");
			}

			return userIdentifier != default;
		}

		private bool TrySearchByLocation(IResponse response, out string userIdentifier)
		{
			var locations = response.Headers.GetValues("Location");
			var location = locations?.FirstOrDefault(l => l.Contains("/login/index.php?testsession"));

			userIdentifier = default;

			if (TryParseLocation(location, out var id) && HasChanged(id))
			{
				userIdentifier = id;
				logger.Info($"User identifier '{id}' detected by location header of response.");
			}

			return userIdentifier != default;
		}

		private bool TrySearchByRequests(IRequest request, IResponse response, out string userIdentifier)
		{
			var id = default(string);
			var type = default(RequestType);
			var cookies = response.Headers.GetValues("Set-Cookie");
			var session = cookies?.FirstOrDefault(c => c.Contains(SESSION_COOKIE_NAME));
			var hasCookie = TryParseCookie(session, out var cookie);
			var hasId = hasCookie && TryExecuteRequests(request.Url, cookie, out type, out id);

			userIdentifier = default;

			if (hasId && HasChanged(id))
			{
				userIdentifier = id;
				logger.Info($"User identifier '{id}' detected by request on response ({type}).");
			}

			return userIdentifier != default;
		}

		private bool TryExecuteRequests(string originUrl, (string name, string value) session, out RequestType requestType, out string userId)
		{
			var order = new[] { RequestType.Plugin, RequestType.Theme };

			requestType = default;
			userId = default;

			foreach (var type in order)
			{
				try
				{
					var url = BuildUrl(originUrl, type);

					using (var response = ExecuteRequest(url, session))
					{
						if (TryParseResponse(response, type, out var id))
						{
							requestType = type;
							userId = id;

							break;
						}
					}
				}
				catch (Exception e)
				{
					logger.Error($"Failed to execute user identifier request ({type})!", e);
				}
			}

			return userId != default;
		}

		private string BuildUrl(string originUrl, RequestType type)
		{
			var uri = new Uri(originUrl);
			var endpointUrl = $"{uri.Scheme}{Uri.SchemeDelimiter}{uri.Host}{(type == RequestType.Plugin ? PLUGIN_PATH : THEME_PATH)}";

			return endpointUrl;
		}

		private HttpResponseMessage ExecuteRequest(string url, (string name, string value) session)
		{
			using (var message = new HttpRequestMessage(HttpMethod.Get, url))
			using (var handler = new HttpClientHandler { UseCookies = false })
			using (var client = new HttpClient(handler))
			{
				message.Headers.Add("Cookie", $"{session.name}={session.value}");

				return client.SendAsync(message).GetAwaiter().GetResult();
			}
		}

		private bool TryParseCookie(string session, out (string name, string value) cookie)
		{
			cookie = default;

			try
			{
				if (session != default)
				{
					var start = session.IndexOf("=") + 1;
					var end = session.IndexOf(";");

					cookie.name = session.Substring(0, start - 1);
					cookie.value = session.Substring(start, end - start);
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to parse session cookie!", e);
			}

			return cookie.name != default && cookie.value != default;
		}

		private bool TryParseLocation(string location, out string userId)
		{
			userId = default;

			try
			{
				if (location != default)
				{
					userId = location.Substring(location.IndexOf("=") + 1);
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to parse location!", e);
			}

			return userId != default;
		}

		private bool TryParseResponse(HttpResponseMessage response, RequestType type, out string userId)
		{
			userId = default;

			if (response.IsSuccessStatusCode)
			{
				var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

				if (int.TryParse(content, out var id) && id > 0)
				{
					userId = content;
				}
			}
			else if (response.StatusCode != HttpStatusCode.NotFound)
			{
				logger.Error($"Failed to retrieve user identifier by request ({type})! Response: {(int) response.StatusCode} {response.ReasonPhrase}");
			}

			return userId != default;
		}

		private enum RequestType
		{
			Plugin,
			Theme
		}
	}
}
