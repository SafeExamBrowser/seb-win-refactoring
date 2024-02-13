/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Net.Http;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Proctoring.ScreenProctoring.Data;
using SafeExamBrowser.Proctoring.ScreenProctoring.Imaging;
using SafeExamBrowser.Proctoring.ScreenProctoring.Service.Requests;

namespace SafeExamBrowser.Proctoring.ScreenProctoring.Service
{
	internal class ServiceProxy
	{
		private readonly Api api;
		private readonly ILogger logger;
		private readonly Parser parser;

		private HttpClient httpClient;

		internal bool IsConnected => SessionId != default;
		internal string SessionId { get; set; }

		internal ServiceProxy(ILogger logger)
		{
			this.api = new Api();
			this.logger = logger;
			this.parser = new Parser(logger);
		}

		internal ServiceResponse Connect(string serviceUrl)
		{
			httpClient = new HttpClient { BaseAddress = new Uri(serviceUrl) };

			var request = new OAuth2TokenRequest(api, httpClient, logger, parser);
			var success = request.TryExecute(out var message);

			if (success)
			{
				logger.Info("Successfully connected to service.");
			}
			else
			{
				logger.Error("Failed to connect to service!");
			}

			return new ServiceResponse(success, message);
		}

		internal ServiceResponse CreateSession(string groupId)
		{
			var request = new CreateSessionRequest(api, httpClient, logger, parser);
			var success = request.TryExecute(groupId, out var message, out var sessionId);

			if (success)
			{
				SessionId = sessionId;
				logger.Info("Successfully created session.");
			}
			else
			{
				logger.Error("Failed to create session!");
			}

			return new ServiceResponse(success, message);
		}

		internal ServiceResponse Send(Metadata metadata, ScreenShot screenShot)
		{
			var request = new ScreenShotRequest(api, httpClient, logger, parser);
			var success = request.TryExecute(metadata, screenShot, SessionId, out var message);

			if (success)
			{
				logger.Info($"Successfully sent screen shot ({screenShot}).");
			}
			else
			{
				logger.Error("Failed to send screen shot!");
			}

			return new ServiceResponse(success, message);
		}

		internal ServiceResponse TerminateSession()
		{
			var request = new TerminateSessionRequest(api, httpClient, logger, parser);
			var success = request.TryExecute(SessionId, out var message);

			if (success)
			{
				SessionId = default;
				logger.Info("Successfully terminated session.");
			}
			else
			{
				logger.Error("Failed to terminate session!");
			}

			return new ServiceResponse(success, message);
		}
	}
}
