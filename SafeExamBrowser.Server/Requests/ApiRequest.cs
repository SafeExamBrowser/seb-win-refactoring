/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Net.Http;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Server.Data;
using SafeExamBrowser.Settings.Server;

namespace SafeExamBrowser.Server.Requests
{
	internal class ApiRequest : Request
	{
		private readonly Sanitizer sanitizer;

		internal ApiRequest(
			Api api,
			HttpClient httpClient,
			ILogger logger,
			Parser parser,
			Sanitizer sanitizer,
			ServerSettings settings) : base(api, httpClient, logger, parser, settings)
		{
			this.sanitizer = sanitizer;
		}

		internal bool TryExecute(out Api api, out string message)
		{
			var success = TryExecute(HttpMethod.Get, settings.ApiUrl, out var response);

			api = new Api();
			message = response.ToLogString();

			if (success)
			{
				parser.TryParseApi(response.Content, out api);
				sanitizer.Sanitize(api);
			}

			return success;
		}
	}
}
