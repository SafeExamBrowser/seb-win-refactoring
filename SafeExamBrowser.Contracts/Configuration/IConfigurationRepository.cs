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
	/// The repository which controls the loading and saving of configuration data.
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
		/// Loads the default settings.
		/// </summary>
		Settings.Settings LoadDefaultSettings();

		/// <summary>
		/// Registers the specified <see cref="IDataFormat"/> as option to parse data from a configuration resource.
		/// </summary>
		void Register(IDataFormat dataFormat);

		/// <summary>
		/// Registers the specified <see cref="IResourceLoader"/> as option to load data from a configuration resource.
		/// </summary>
		void Register(IResourceLoader resourceLoader);

		/// <summary>
		/// Attempts to load settings from the specified resource. As long as the result is not <see cref="LoadStatus.Success"/>,
		/// the referenced settings may be <c>null</c> or in an undefinable state!
		/// </summary>
		LoadStatus TryLoadSettings(Uri resource, PasswordInfo passwordInfo, out Settings.Settings settings);
	}
}
