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
	internal class ExamConfigurationRequest : BaseRequest
	{
		internal ExamConfigurationRequest(
			ApiVersion1 api,
			HttpClient httpClient,
			ILogger logger,
			Parser parser,
			ServerSettings settings) : base(api, httpClient, logger, parser, settings)
		{
		}

		internal bool TryExecute(Exam exam, out HttpContent content, out string message)
		{
			var url = $"{api.ConfigurationEndpoint}?examId={exam.Id}";
			var success = TryExecute(HttpMethod.Get, url, out var response, default, default, Authorization, Token);

			content = response.Content;
			message = response.ToLogString();

			return success;
		}
	}
}
