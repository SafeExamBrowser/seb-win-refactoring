/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
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
using SafeExamBrowser.Server.Requests;

namespace SafeExamBrowser.Server
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

		internal bool TryParseAppSignatureKeySalt(HttpResponseMessage response, out string salt)
		{
			salt = default;

			try
			{
				var hasHeader = response.Headers.TryGetValues(Header.APP_SIGNATURE_KEY_SALT, out var values);

				if (hasHeader)
				{
					salt = values.First();
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to parse app signature key salt!", e);
			}

			return salt != default;
		}

		internal bool TryParseBrowserExamKey(HttpResponseMessage response, out string browserExamKey)
		{
			browserExamKey = default;

			try
			{
				var hasHeader = response.Headers.TryGetValues(Header.BROWSER_EXAM_KEY, out var values);

				if (hasHeader)
				{
					browserExamKey = values.First();
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to parse browser exam key!", e);
			}

			return browserExamKey != default;
		}

		internal bool TryParseConnectionToken(HttpResponseMessage response, out string connectionToken)
		{
			connectionToken = default;

			try
			{
				var hasHeader = response.Headers.TryGetValues(Header.CONNECTION_TOKEN, out var values);

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

			return connectionToken != default;
		}

		internal bool TryParseExams(HttpContent content, out IEnumerable<Exam> exams)
		{
			var list = new List<Exam>();

			try
			{
				var json = JsonConvert.DeserializeObject(Extract(content)) as JArray;

				foreach (var exam in json.AsJEnumerable())
				{
					list.Add(new Exam
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

			exams = list;

			return exams.Any();
		}

		internal bool TryParseInstruction(HttpContent content, out Attributes attributes, out string instruction, out string instructionConfirmation)
		{
			attributes = new Attributes();
			instruction = default;
			instructionConfirmation = default;

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

						attributes = ParseAttributes(attributesJson, instruction);
					}
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to parse instruction!", e);
			}

			return instruction != default;
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

		private Attributes ParseAttributes(JObject attributesJson, string instruction)
		{
			var attributes = new Attributes();

			switch (instruction)
			{
				case Instructions.LOCK_SCREEN:
					ParseLockScreenInstruction(attributes, attributesJson);
					break;
				case Instructions.NOTIFICATION_CONFIRM:
					ParseNotificationConfirmation(attributes, attributesJson);
					break;
				case Instructions.PROCTORING:
					ParseProctoringInstruction(attributes, attributesJson);
					break;
				case Instructions.PROCTORING_RECONFIGURATION:
					ParseReconfigurationInstruction(attributes, attributesJson);
					break;
			}

			return attributes;
		}

		private void ParseLockScreenInstruction(Attributes attributes, JObject attributesJson)
		{
			if (attributesJson.ContainsKey("message"))
			{
				attributes.Message = attributesJson["message"].Value<string>();
			}
		}

		private void ParseNotificationConfirmation(Attributes attributes, JObject attributesJson)
		{
			if (attributesJson.ContainsKey("id"))
			{
				attributes.Id = attributesJson["id"].Value<int>();
			}

			if (attributesJson.ContainsKey("type"))
			{
				switch (attributesJson["type"].Value<string>())
				{
					case "lockscreen":
						attributes.Type = AttributeType.LockScreen;
						break;
					case "raisehand":
						attributes.Type = AttributeType.Hand;
						break;
				}
			}
		}

		private void ParseProctoringInstruction(Attributes attributes, JObject attributesJson)
		{
			var provider = attributesJson["service-type"].Value<string>();

			switch (provider)
			{
				case "JITSI_MEET":
					attributes.Instruction.JitsiMeetRoomName = attributesJson["jitsiMeetRoom"].Value<string>();
					attributes.Instruction.JitsiMeetServerUrl = attributesJson["jitsiMeetServerURL"].Value<string>();
					attributes.Instruction.JitsiMeetToken = attributesJson["jitsiMeetToken"].Value<string>();
					break;
				case "ZOOM":
					attributes.Instruction.ZoomMeetingNumber = attributesJson["zoomRoom"].Value<string>();
					attributes.Instruction.ZoomPassword = attributesJson["zoomMeetingKey"].Value<string>();
					attributes.Instruction.ZoomSdkKey = attributesJson["zoomAPIKey"].Value<string>();
					attributes.Instruction.ZoomSignature = attributesJson["zoomToken"].Value<string>();
					attributes.Instruction.ZoomSubject = attributesJson["zoomSubject"].Value<string>();
					attributes.Instruction.ZoomUserName = attributesJson["zoomUserName"].Value<string>();
					break;
			}
		}

		private void ParseReconfigurationInstruction(Attributes attributes, JObject attributesJson)
		{
			if (attributesJson.ContainsKey("jitsiMeetFeatureFlagChat"))
			{
				attributes.AllowChat = attributesJson["jitsiMeetFeatureFlagChat"].Value<bool>();
			}

			if (attributesJson.ContainsKey("zoomFeatureFlagChat"))
			{
				attributes.AllowChat = attributesJson["zoomFeatureFlagChat"].Value<bool>();
			}

			if (attributesJson.ContainsKey("jitsiMeetReceiveAudio"))
			{
				attributes.ReceiveAudio = attributesJson["jitsiMeetReceiveAudio"].Value<bool>();
			}

			if (attributesJson.ContainsKey("zoomReceiveAudio"))
			{
				attributes.ReceiveAudio = attributesJson["zoomReceiveAudio"].Value<bool>();
			}

			if (attributesJson.ContainsKey("jitsiMeetReceiveVideo"))
			{
				attributes.ReceiveVideo = attributesJson["jitsiMeetReceiveVideo"].Value<bool>();
			}

			if (attributesJson.ContainsKey("zoomReceiveVideo"))
			{
				attributes.ReceiveVideo = attributesJson["zoomReceiveVideo"].Value<bool>();
			}
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
