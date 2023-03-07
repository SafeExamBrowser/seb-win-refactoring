/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Settings.Browser.Proxy
{
	/// <summary>
	/// Defines the configuration of a proxy server.
	/// </summary>
	[Serializable]
	public class ProxyConfiguration
	{
		/// <summary>
		/// The host name or IP address of the proxy server.
		/// </summary>
		public string Host { get; set; }

		/// <summary>
		/// The password to be used for authentication.
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// The port of the proxy server.
		/// </summary>
		public int Port { get; set; }

		/// <summary>
		/// The protocol of the proxy server.
		/// </summary>
		public ProxyProtocol Protocol { get; set; }

		/// <summary>
		/// Determines whether the proxy server requires authentication.
		/// </summary>
		public bool RequiresAuthentication { get; set; }

		/// <summary>
		/// The username to be used for authentication.
		/// </summary>
		public string Username { get; set; }
	}
}
