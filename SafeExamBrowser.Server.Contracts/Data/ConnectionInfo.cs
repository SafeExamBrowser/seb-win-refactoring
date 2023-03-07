/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Server.Contracts.Data
{
	/// <summary>
	/// Contains all information required to establish a connection with a server.
	/// </summary>
	public class ConnectionInfo
	{
		/// <summary>
		/// The API of the server as JSON string.
		/// </summary>
		public string Api { get; set; }

		/// <summary>
		/// The connection token for authentication with the server.
		/// </summary>
		public string ConnectionToken { get; set; }

		/// <summary>
		/// The OAuth2 token for authentication with the server.
		/// </summary>
		public string Oauth2Token { get; set; }
	}
}
