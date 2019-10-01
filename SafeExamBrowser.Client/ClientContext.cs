/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Settings;

namespace SafeExamBrowser.Client
{
	/// <summary>
	/// Holds all configuration and runtime data for the client.
	/// </summary>
	internal class ClientContext
	{
		/// <summary>
		/// The global application configuration.
		/// </summary>
		internal AppConfig AppConfig { get; set; }

		/// <summary>
		/// The settings for the current session.
		/// </summary>
		internal AppSettings Settings { get; set; }
	}
}
