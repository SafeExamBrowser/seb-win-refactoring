/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
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
	/// The configuration for a session of the service application component.
	/// </summary>
	[Serializable]
	public class ServiceConfiguration
	{
		/// <summary>
		/// The global application configuration.
		/// </summary>
		public AppConfig AppConfig { get; set; }

		/// <summary>
		/// The unique identifier for the current session.
		/// </summary>
		public Guid SessionId { get; set; }

		/// <summary>
		/// The application settings to be used by the service.
		/// </summary>
		public AppSettings Settings { get; set; }

		/// <summary>
		/// The user name of the currently logged in user.
		/// </summary>
		public string UserName { get; set; }

		/// <summary>
		/// The security identifier of the currently logged in user.
		/// </summary>
		public string UserSid { get; set; }
	}
}
