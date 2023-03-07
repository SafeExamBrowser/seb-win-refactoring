/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Server.Data
{
	internal class ApiVersion1
	{
		public string AccessTokenEndpoint { get; set; }
		public string HandshakeEndpoint { get; set; }
		public string ConfigurationEndpoint { get; set; }
		public string PingEndpoint { get; set; }
		public string LogEndpoint { get; set; }
	}
}
