/*
 * Copyright (c) 2025 ETH Zürich, IT Services
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

namespace SafeExamBrowser.Proctoring.ScreenProctoring.Service.Requests
{
	internal abstract class Request
	{
		private const int ATTEMPTS = 5;

		private static string oauth2Token;

		private readonly HttpClient httpClient;

		private bool hadException;

		protected readonly Api api;
		protected readonly ILogger logger;
		protected readonly Parser parser;

		protected static string ClientId { get; set; }
		protected static string ClientSecret { get; set; }

		protected (string, string) Authorization => (Header.AUTHORIZATION, $"Bearer {oauth2Token}");

		protected Request(Api api, HttpClient httpClient, ILogger logger, Parser parser)
		{
			this.api = api;
			this.httpClient = httpClient;
			this.logger = logger;
			this.parser = parser;
		}

		protected bool TryExecute(
			HttpMethod method,
			string url,
			out HttpResponseMessage response,
			object content = default,
			string contentType = default,
			params (string name, string value)[] headers)
		{
			response = default;

			for (var attempt = 0; attempt < ATTEMPTS && (response == default || !response.IsSuccessStatusCode); attempt++)
			{
				var request = BuildRequest(method, url, content, contentType, headers);

				try
				{
					response = httpClient.SendAsync(request).GetAwaiter().GetResult();

					logger.Debug($"Completed request: {request.Method} '{request.RequestUri}' -> {response.ToLogString()}");

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
					logger.Warn($"Request {request.Method} '{request.RequestUri}' did not complete within {httpClient.Timeout}!");

					break;
				}
				catch (Exception e)
				{
					if (IsFirstException())
					{
						logger.Warn($"Request {request.Method} '{request.RequestUri}' has failed: {e.ToSummary()}!");
					}
				}
			}

			return response != default && response.IsSuccessStatusCode;
		}

		protected bool TryRetrieveOAuth2Token(out string message)
		{
			var secret = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ClientId}:{ClientSecret}"));
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
			object content = default,
			string contentType = default,
			params (string name, string value)[] headers)
		{
			var request = new HttpRequestMessage(method, url);

			if (content != default)
			{
				if (content is string)
				{
					request.Content = new StringContent(content as string, Encoding.UTF8);
				}

				if (content is byte[])
				{
					request.Content = new ByteArrayContent(content as byte[]);
				}

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

		private bool IsFirstException()
		{
			var isFirst = !hadException;

			hadException = true;

			return isFirst;
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
