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
		/// Initializes the global configuration information for the currently running application instance.
		/// </summary>
		AppConfig InitializeAppConfig();

		/// <summary>
		/// Initializes all relevant configuration data for a new session.
		/// </summary>
		ISessionConfiguration InitializeSessionConfiguration();

		/// <summary>
		/// Attempts to load settings from the specified resource, using the optional passwords. Returns a <see cref="LoadStatus"/>
		/// indicating the result of the operation. As long as the result is not <see cref="LoadStatus.Success"/>, the declared
		/// <paramref name="settings"/> will be <c>null</c>!
		/// </summary>
		LoadStatus TryLoadSettings(Uri resource, out Settings.Settings settings, string adminPassword = null, string settingsPassword = null);

		/// <summary>
		/// Loads the default settings.
		/// </summary>
		Settings.Settings LoadDefaultSettings();
	}
}
