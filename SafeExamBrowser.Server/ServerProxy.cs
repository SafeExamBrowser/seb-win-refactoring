/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Server.Contracts;
using SafeExamBrowser.Server.Data;
using SafeExamBrowser.Settings.Server;

namespace SafeExamBrowser.Server
{
	public class ServerProxy : IServerProxy
	{
		private ApiVersion1 api;
		private string connectionToken;
		private HttpClient httpClient;
		private ILogger logger;
		private string oauth2Token;
		private ServerSettings settings;

		public ServerProxy(ILogger logger)
		{
			this.api = new ApiVersion1();
			this.httpClient = new HttpClient();
			this.logger = logger;
		}

		public ServerResponse<string> Connect()
		{
			var success = TryExecute(HttpMethod.Get, settings.ApiUrl, out var response);
			var message = ToString(response);

			if (success && TryParseApi(response.Content))
			{
				logger.Info("Successfully loaded server API.");

				var secret = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{settings.ClientName}:{settings.ClientSecret}"));
				var authorization = ("Authorization", $"Basic {secret}");
				var content = "grant_type=client_credentials&scope=read write";
				var contentType = "application/x-www-form-urlencoded";

				success = TryExecute(HttpMethod.Post, api.AccessTokenEndpoint, out response, content, contentType, authorization);
				message = ToString(response);

				if (success && TryParseOauth2Token(response.Content))
				{
					logger.Info("Successfully retrieved OAuth2 token.");
				}
				else
				{
					logger.Error("Failed to retrieve OAuth2 token!");
				}
			}
			else
			{
				logger.Error("Failed to load server API!");
			}

			return new ServerResponse<string>(success, oauth2Token, message);
		}

		public ServerResponse Disconnect()
		{
			return new ServerResponse(false, "Some error message here");
		}

		public ServerResponse<IEnumerable<Exam>> GetAvailableExams()
		{
			var authorization = ("Authorization", $"Bearer {oauth2Token}");
			var content = $"institutionId={settings.Institution}";
			var contentType = "application/x-www-form-urlencoded";

			var success = TryExecute(HttpMethod.Post, api.HandshakeEndpoint, out var response, content, contentType, authorization);
			var message = ToString(response);
			var hasToken = TryParseConnectionToken(response);
			var hasExams = TryParseExams(response.Content, out var exams);

			if (success && hasExams && hasToken)
			{
				logger.Info("Successfully retrieved connection token and available exams.");
			}
			else if (!hasExams)
			{
				logger.Error("Failed to retrieve available exams!");
			}
			else if (!hasToken)
			{
				logger.Error("Failed to retrieve connection token!");
			}
			else
			{
				logger.Error("Failed to load connection token and available exams!");
			}

			return new ServerResponse<IEnumerable<Exam>>(hasExams && hasToken, exams, message);
		}

		public ServerResponse<Uri> GetConfigurationFor(Exam exam)
		{
			// 4. Send exam ID

			return new ServerResponse<Uri>(false, default(Uri), "Some error message here");
		}

		public void Initialize(ServerSettings settings)
		{
			this.settings = settings;
			httpClient.BaseAddress = new Uri(settings.ServerUrl);

			if (settings.RequestTimeout > 0)
			{
				httpClient.Timeout = TimeSpan.FromMilliseconds(settings.RequestTimeout);
			}
		}

		public ServerResponse SendSessionInfo(string sessionId)
		{
			return new ServerResponse(false, "Some error message here");
		}

		private bool TryParseApi(HttpContent content)
		{
			var success = false;

			try
			{
				var json = JsonConvert.DeserializeObject(Extract(content)) as JObject;
				var apis = json["api-versions"];

				foreach (var api in apis.AsJEnumerable())
				{
					if (api["name"].Value<string>().Equals("v1"))
					{
						foreach (var endpoint in api["endpoints"].AsJEnumerable())
						{
							var name = endpoint["name"].Value<string>();
							var location = endpoint["location"].Value<string>();

							switch (name)
							{
								case "access-token-endpoint":
									this.api.AccessTokenEndpoint = location;
									break;
								case "seb-configuration-endpoint":
									this.api.ConfigurationEndpoint = location;
									break;
								case "seb-handshake-endpoint":
									this.api.HandshakeEndpoint = location;
									break;
								case "seb-log-endpoint":
									this.api.LogEndpoint = location;
									break;
								case "seb-ping-endpoint":
									this.api.PingEndpoint = location;
									break;
							}
						}

						success = true;
					}

					if (!success)
					{
						logger.Error("The selected SEB server instance does not support the required API version!");
					}
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to parse server API!", e);
			}

			return success;
		}

		private bool TryParseConnectionToken(HttpResponseMessage response)
		{
			try
			{
				var hasHeader = response.Headers.TryGetValues("SEBConnectionToken", out var values);

				if (hasHeader)
				{
					connectionToken = values.First();
				}
				else
				{
					logger.Error("Failed to retrieve connection token!");
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to parse connection token!", e);
			}

			return connectionToken != default(string);
		}

		private bool TryParseExams(HttpContent content, out IList<Exam> exams)
		{
			exams = new List<Exam>();

			try
			{
				var json = JsonConvert.DeserializeObject(Extract(content)) as JArray;

				foreach (var exam in json.AsJEnumerable())
				{
					exams.Add(new Exam
					{
						Id = exam["examId"].Value<string>(),
						LmsName = exam["lmsType"].Value<string>(),
						Name = exam["name"].Value<string>(),
						Url = exam["url"].Value<string>()
					});
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to parse exams!", e);
			}

			return exams.Any();
		}

		private bool TryParseOauth2Token(HttpContent content)
		{
			try
			{
				var json = JsonConvert.DeserializeObject(Extract(content)) as JObject;

				oauth2Token = json["access_token"].Value<string>();
			}
			catch (Exception e)
			{
				logger.Error("Failed to parse Oauth2 token!", e);
			}

			return oauth2Token != default(string);
		}

		private bool TryExecute(
			HttpMethod method,
			string url,
			out HttpResponseMessage response,
			string content = default(string),
			string contentType = default(string),
			params (string name, string value)[] headers)
		{
			response = default(HttpResponseMessage);

			for (var attempt = 0; attempt < settings.RequestAttempts && response == default(HttpResponseMessage); attempt++)
			{
				var request = new HttpRequestMessage(method, url);

				if (content != default(string))
				{
					request.Content = new StringContent(content, Encoding.UTF8);

					if (contentType != default(string))
					{
						request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
					}
				}

				foreach (var (name, value) in headers)
				{
					request.Headers.Add(name, value);
				}

				try
				{
					response = httpClient.SendAsync(request).GetAwaiter().GetResult();
					logger.Debug($"Request was successful: {request.Method} '{request.RequestUri}' -> {ToString(response)}");
				}
				catch (TaskCanceledException)
				{
					logger.Error($"Request {request.Method} '{request.RequestUri}' did not complete within {settings.RequestTimeout}ms!");
					break;
				}
				catch (Exception e)
				{
					logger.Debug($"Request {request.Method} '{request.RequestUri}' failed due to {e}");
				}
			}

			return response != default(HttpResponseMessage) && response.IsSuccessStatusCode;
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

		private string ToString(HttpResponseMessage response)
		{
			return $"{(int?) response?.StatusCode} {response?.StatusCode} {response?.ReasonPhrase}";
		}
	}
}
