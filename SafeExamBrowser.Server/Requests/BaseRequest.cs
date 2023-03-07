/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Server.Data;
using SafeExamBrowser.Settings.Server;

namespace SafeExamBrowser.Server.Requests
{
	internal abstract class BaseRequest
	{
		private static string connectionToken;
		private static string oauth2Token;

		private readonly HttpClient httpClient;

		protected readonly ApiVersion1 api;
		protected readonly ILogger logger;
		protected readonly Parser parser;
		protected readonly ServerSettings settings;

		protected (string, string) Authorization => (Header.AUTHORIZATION, $"Bearer {oauth2Token}");
		protected (string, string) Token => (Header.CONNECTION_TOKEN, connectionToken);

		internal static string ConnectionToken
		{
			get { return connectionToken; }
			set { connectionToken = value; }
		}

		internal static string Oauth2Token
		{
			get { return oauth2Token; }
			set { oauth2Token = value; }
		}

		protected BaseRequest(ApiVersion1 api, HttpClient httpClient, ILogger logger, Parser parser, ServerSettings settings)
		{
			this.api = api;
			this.httpClient = httpClient;
			this.logger = logger;
			this.parser = parser;
			this.settings = settings;
		}

		protected bool TryExecute(
			HttpMethod method,
			string url,
			out HttpResponseMessage response,
			string content = default,
			string contentType = default,
			params (string name, string value)[] headers)
		{
			response = default;

			for (var attempt = 0; attempt < settings.RequestAttempts && (response == default || !response.IsSuccessStatusCode); attempt++)
			{
				var request = BuildRequest(method, url, content, contentType, headers);

				try
				{
					response = httpClient.SendAsync(request).GetAwaiter().GetResult();

					if (request.RequestUri.AbsolutePath != api.LogEndpoint && request.RequestUri.AbsolutePath != api.PingEndpoint)
					{
						logger.Debug($"Completed request: {request.Method} '{request.RequestUri}' -> {response.ToLogString()}");
					}

					if (response.StatusCode == HttpStatusCode.Unauthorized && parser.IsTokenExpired(response.Content))
					{
						logger.Info("OAuth2 token has expired, attempting to retrieve new one...");

						if (TryRetrieveOAuth2Token(out var message))
						{
							headers = UpdateOAuth2Token(headers);
						}
					}
				}
				catch (TaskCanceledException)
				{
					logger.Debug($"Request {request.Method} '{request.RequestUri}' did not complete within {settings.RequestTimeout}ms!");
					break;
				}
				catch (Exception e)
				{
					logger.Debug($"Request {request.Method} '{request.RequestUri}' failed due to {e}");
				}
			}

			return response != default && response.IsSuccessStatusCode;
		}

		protected bool TryRetrieveConnectionToken(HttpResponseMessage response)
		{
			var success = parser.TryParseConnectionToken(response, out connectionToken);

			if (success)
			{
				logger.Info("Successfully retrieved connection token.");
			}
			else
			{
				logger.Error("Failed to retrieve connection token!");
			}

			return success;
		}

		protected bool TryRetrieveOAuth2Token(out string message)
		{
			var secret = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{settings.ClientName}:{settings.ClientSecret}"));
			var authorization = (Header.AUTHORIZATION, $"Basic {secret}");
			var content = "grant_type=client_credentials&scope=read write";
			var success = TryExecute(HttpMethod.Post, api.AccessTokenEndpoint, out var response, content, ContentType.URL_ENCODED, authorization);

			message = response.ToLogString();

			if (success && parser.TryParseOauth2Token(response.Content, out oauth2Token))
			{
				logger.Info("Successfully retrieved OAuth2 token.");
			}
			else
			{
				logger.Error("Failed to retrieve OAuth2 token!");
			}

			return success;
		}

		private HttpRequestMessage BuildRequest(
			HttpMethod method,
			string url,
			string content = default,
			string contentType = default,
			params (string name, string value)[] headers)
		{
			var request = new HttpRequestMessage(method, url);

			if (content != default)
			{
				request.Content = new StringContent(content, Encoding.UTF8);

				if (contentType != default)
				{
					request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
				}
			}

			request.Headers.Add(Header.ACCEPT, "application/json, */*");

			foreach (var (name, value) in headers)
			{
				request.Headers.Add(name, value);
			}

			return request;
		}

		private (string name, string value)[] UpdateOAuth2Token((string name, string value)[] headers)
		{
			var result = new List<(string name, string value)>();

			foreach (var header in headers)
			{
				if (header.name == Header.AUTHORIZATION)
				{
					result.Add((Header.AUTHORIZATION, $"Bearer {oauth2Token}"));
				}
				else
				{
					result.Add(header);
				}
			}

			return result.ToArray();
		}
	}
}
