/*
 * Copyright (c) 2021 ETH Zürich, Educational Development and Technology (LET)
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
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Server.Contracts.Data;
using SafeExamBrowser.Server.Data;

namespace SafeExamBrowser.Server
{
	internal class Parser
	{
		private readonly ILogger logger;

		internal Parser(ILogger logger)
		{
			this.logger = logger;
		}

		internal bool TryParseApi(HttpContent content, out ApiVersion1 api)
		{
			var success = false;

			api = new ApiVersion1();

			try
			{
				var json = JsonConvert.DeserializeObject(Extract(content)) as JObject;
				var apisJson = json["api-versions"];

				foreach (var apiJson in apisJson.AsJEnumerable())
				{
					if (apiJson["name"].Value<string>().Equals("v1"))
					{
						foreach (var endpoint in apiJson["endpoints"].AsJEnumerable())
						{
							var name = endpoint["name"].Value<string>();
							var location = endpoint["location"].Value<string>();

							switch (name)
							{
								case "access-token-endpoint":
									api.AccessTokenEndpoint = location;
									break;
								case "seb-configuration-endpoint":
									api.ConfigurationEndpoint = location;
									break;
								case "seb-handshake-endpoint":
									api.HandshakeEndpoint = location;
									break;
								case "seb-log-endpoint":
									api.LogEndpoint = location;
									break;
								case "seb-ping-endpoint":
									api.PingEndpoint = location;
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

		internal bool TryParseConnectionToken(HttpResponseMessage response, out string connectionToken)
		{
			connectionToken = default(string);

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

		internal bool TryParseExams(HttpContent content, out IList<Exam> exams)
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

		internal bool TryParseInstruction(HttpContent content, out Attributes attributes, out string instruction, out string instructionConfirmation)
		{
			attributes = new Attributes();
			instruction = default(string);
			instructionConfirmation = default(string);

			try
			{
				var json = JsonConvert.DeserializeObject(Extract(content)) as JObject;

				if (json != default(JObject))
				{
					instruction = json["instruction"].Value<string>();

					if (json.ContainsKey("attributes"))
					{
						var attributesJson = json["attributes"] as JObject;

						if (attributesJson.ContainsKey("instruction-confirm"))
						{
							instructionConfirmation = attributesJson["instruction-confirm"].Value<string>();
						}

						switch (instruction)
						{
							case Instructions.PROCTORING:
								attributes.RoomName = attributesJson["jitsiMeetRoom"].Value<string>();
								attributes.ServerUrl = attributesJson["jitsiMeetServerURL"].Value<string>();
								attributes.Token = attributesJson["jitsiMeetToken"].Value<string>();
								break;
							case Instructions.PROCTORING_RECONFIGURATION:
								attributes.AllowChat = attributesJson["jitsiMeetFeatureFlagChat"].Value<bool>();
								attributes.ReceiveAudio = attributesJson["jitsiMeetReceiveAudio"].Value<bool>();
								attributes.ReceiveVideo = attributesJson["jitsiMeetReceiveVideo"].Value<bool>();
								break;
						}
					}
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to parse instruction!", e);
			}

			return instruction != default(string);
		}

		internal bool TryParseOauth2Token(HttpContent content, out string oauth2Token)
		{
			oauth2Token = default(string);

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
