/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Settings;

namespace SafeExamBrowser.Configuration.Contracts
{
	/// <summary>
	/// Container holding all session-related configuration data.
	/// </summary>
	public class SessionConfiguration
	{
		/// <summary>
		/// The application configuration for this session.
		/// </summary>
		public AppConfig AppConfig { get; set; }

		/// <summary>
		/// The token used for initial communication authentication with the client.
		/// </summary>
		public Guid ClientAuthenticationToken { get; set; }

		/// <summary>
		/// Indicates whether a configuration resource needs to be loaded in the browser because it requires authentication or is a webpage.
		/// </summary>
		public bool IsBrowserResource { get; set; }

		/// <summary>
		/// The unique session identifier.
		/// </summary>
		public Guid SessionId { get; set; }

		/// <summary>
		/// The settings used for this session.
		/// </summary>
		public AppSettings Settings { get; set; }
	}
}
