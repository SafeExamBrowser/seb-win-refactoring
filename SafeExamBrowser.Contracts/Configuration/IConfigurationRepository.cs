/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Contracts.Configuration
{
	/// <summary>
	/// The repository which controls the loading and initializing of configuration data.
	/// </summary>
	public interface IConfigurationRepository
	{
		/// <summary>
		/// The global configuration information for the currently running application instance.
		/// </summary>
		AppConfig AppConfig { get; }

		/// <summary>
		/// Retrieves the current session data, i.e. the last one which was initialized. If no session has been initialized yet, this
		/// property will be <c>null</c>!
		/// </summary>
		ISessionData CurrentSession { get; }

		/// <summary>
		/// Retrieves the current settings, i.e. the last ones which were loaded. If no settings have been loaded yet, this property will
		/// be <c>null</c>!
		/// </summary>
		Settings.Settings CurrentSettings { get; }

		/// <summary>
		/// The path of the settings file to be used when reconfiguring the application.
		/// </summary>
		string ReconfigurationFilePath { get; set; }

		/// <summary>
		/// Builds a configuration for the client component, given the currently loaded settings, session and runtime information.
		/// </summary>
		ClientConfiguration BuildClientConfiguration();

		/// <summary>
		/// Initializes all relevant data for a new session.
		/// </summary>
		void InitializeSessionConfiguration();

		/// <summary>
		/// Attempts to load settings from the specified resource, using the optional passwords. Returns a <see cref="LoadStatus"/>
		/// indicating the result of the operation.
		/// </summary>
		LoadStatus LoadSettings(Uri resource, string adminPassword = null, string settingsPassword = null);

		/// <summary>
		/// Loads the default settings.
		/// </summary>
		void LoadDefaultSettings();
	}
}
