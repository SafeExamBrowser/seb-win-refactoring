/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Configuration.Settings;

namespace SafeExamBrowser.Contracts.Configuration
{
	public interface IClientConfiguration
	{
		/// <summary>
		/// The session data to be used by the client.
		/// </summary>
		ISessionData SessionData { get; set; }

		/// <summary>
		/// The application settings to be used by the client.
		/// </summary>
		ISettings Settings { get; set; }

		/// <summary>
		/// The information about the current runtime.
		/// </summary>
		IRuntimeInfo RuntimeInfo { get; set; }
	}
}
