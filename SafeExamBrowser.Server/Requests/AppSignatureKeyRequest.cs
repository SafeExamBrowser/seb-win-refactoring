/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
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
	internal class AppSignatureKeyRequest : BaseRequest
	{
		internal AppSignatureKeyRequest(
			ApiVersion1 api,
			HttpClient httpClient,
			ILogger logger,
			Parser parser,
			ServerSettings settings) : base(api, httpClient, logger, parser, settings)
		{
		}

		internal bool TryExecute(string appSignatureKey, out string message)
		{
			var content = $"seb_signature_key={appSignatureKey}";
			var success = TryExecute(new HttpMethod("PATCH"), api.HandshakeEndpoint, out var response, content, ContentType.URL_ENCODED, Authorization, Token);

			message = response.ToLogString();

			return success;
		}
	}
}
