/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Contracts.Configuration
{
	/// <summary>
	/// Holds all session-related configuration data.
	/// </summary>
	public interface ISessionConfiguration
	{
		/// <summary>
		/// The active application configuration for this session.
		/// </summary>
		AppConfig AppConfig { get; }

		/// <summary>
		/// The unique session identifier.
		/// </summary>
		Guid Id { get; }

		/// <summary>
		/// The settings used for this session.
		/// </summary>
		Settings.Settings Settings { get; set; }

		/// <summary>
		/// The startup token used by the client and runtime components for initial authentication.
		/// </summary>
		Guid StartupToken { get; }
	}
}
