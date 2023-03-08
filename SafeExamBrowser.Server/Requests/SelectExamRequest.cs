/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Net.Http;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Server.Contracts.Data;
using SafeExamBrowser.Server.Data;
using SafeExamBrowser.Settings.Server;

namespace SafeExamBrowser.Server.Requests
{
	internal class SelectExamRequest : BaseRequest
	{
		internal SelectExamRequest(ApiVersion1 api, HttpClient httpClient, ILogger logger, Parser parser, ServerSettings settings) : base(api, httpClient, logger, parser, settings)
		{
		}

		internal bool TryExecute(Exam exam, out string message, out string appSignatureKeySalt, out string browserExamKey)
		{
			var content = $"examId={exam.Id}";
			var method = new HttpMethod("PATCH");
			var success = TryExecute(method, api.HandshakeEndpoint, out var response, content, ContentType.URL_ENCODED, Authorization, Token);

			appSignatureKeySalt = default;
			browserExamKey = default;
			message = response.ToLogString();

			if (success)
			{
				parser.TryParseAppSignatureKeySalt(response, out appSignatureKeySalt);
				parser.TryParseBrowserExamKey(response, out browserExamKey);
			}

			return success;
		}
	}
}
