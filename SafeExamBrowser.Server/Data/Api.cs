/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using Newtonsoft.Json;

namespace SafeExamBrowser.Server.Data
{
	internal class Api
	{
		[JsonProperty]
		internal string AccessTokenEndpoint { get; set; }

		[JsonProperty]
		internal string HandshakeEndpoint { get; set; }

		[JsonProperty]
		internal string ConfigurationEndpoint { get; set; }

		[JsonProperty]
		internal string PingEndpoint { get; set; }

		[JsonProperty]
		internal string LogEndpoint { get; set; }
	}
}
