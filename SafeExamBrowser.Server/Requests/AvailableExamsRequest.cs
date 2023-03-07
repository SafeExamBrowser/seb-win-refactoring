/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Net.Http;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Server.Contracts.Data;
using SafeExamBrowser.Server.Data;
using SafeExamBrowser.Settings.Server;
using SafeExamBrowser.SystemComponents.Contracts;

namespace SafeExamBrowser.Server.Requests
{
	internal class AvailableExamsRequest : BaseRequest
	{
		private readonly AppConfig appConfig;
		private readonly ISystemInfo systemInfo;
		private readonly IUserInfo userInfo;

		internal AvailableExamsRequest(
			ApiVersion1 api,
			AppConfig appConfig,
			HttpClient httpClient,
			ILogger logger,
			Parser parser,
			ServerSettings settings,
			ISystemInfo systemInfo,
			IUserInfo userInfo) : base(api, httpClient, logger, parser, settings)
		{
			this.appConfig = appConfig;
			this.systemInfo = systemInfo;
			this.userInfo = userInfo;
		}

		internal bool TryExecute(string examId, out IEnumerable<Exam> exams, out string message)
		{
			var clientInfo = $"client_id={userInfo.GetUserName()}&seb_machine_name={systemInfo.Name}";
			var versionInfo = $"seb_os_name={systemInfo.OperatingSystemInfo}&seb_version={appConfig.ProgramInformationalVersion}";
			var content = $"institutionId={settings.Institution}&{clientInfo}&{versionInfo}{(examId == default ? "" : $"&examId={examId}")}";

			var success = TryExecute(HttpMethod.Post, api.HandshakeEndpoint, out var response, content, ContentType.URL_ENCODED, Authorization);

			exams = default;
			message = response.ToLogString();

			if (success)
			{
				var hasExams = parser.TryParseExams(response.Content, out exams);
				var hasToken = TryRetrieveConnectionToken(response);

				success = hasExams && hasToken;
			}

			return success;
		}
	}
}
