/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Configuration.Contracts.Cryptography;
using SafeExamBrowser.Configuration.Contracts.DataFormats;
using SafeExamBrowser.Configuration.Contracts.DataResources;
using SafeExamBrowser.Settings;

namespace SafeExamBrowser.Configuration.Contracts
{
	/// <summary>
	/// The repository which controls the loading and saving of configuration data.
	/// </summary>
	public interface IConfigurationRepository
	{
		/// <summary>
		/// Attempts to save the given resource as local client configuration.
		/// </summary>
		SaveStatus ConfigureClientWith(Uri resource, PasswordParameters password = null);

		/// <summary>
		/// Initializes the global configuration information for the currently running application instance.
		/// </summary>
		AppConfig InitializeAppConfig();

		/// <summary>
		/// Initializes all relevant configuration data for a new session.
		/// </summary>
		SessionConfiguration InitializeSessionConfiguration();

		/// <summary>
		/// Loads the default settings.
		/// </summary>
		AppSettings LoadDefaultSettings();

		/// <summary>
		/// Registers the specified <see cref="IDataParser"/> to be used to parse configuration data.
		/// </summary>
		void Register(IDataParser parser);

		/// <summary>
		/// Registers the specified <see cref="IDataSerializer"/> to be used to serialize configuration data.
		/// </summary>
		void Register(IDataSerializer serializer);

		/// <summary>
		/// Registers the specified <see cref="IResourceLoader"/> to be used to load configuration resources.
		/// </summary>
		void Register(IResourceLoader loader);

		/// <summary>
		/// Registers the specified <see cref="IResourceSaver"/> to be used to save configuration resources.
		/// </summary>
		void Register(IResourceSaver saver);

		/// <summary>
		/// Attempts to load settings from the specified resource.
		/// </summary>
		LoadStatus TryLoadSettings(Uri resource, out AppSettings settings, PasswordParameters password = null);
	}
}
