/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Configuration.Cryptography;
using SafeExamBrowser.Contracts.Configuration.DataFormats;
using SafeExamBrowser.Contracts.Configuration.DataResources;

namespace SafeExamBrowser.Contracts.Configuration
{
	/// <summary>
	/// The repository which controls the loading and saving of configuration data.
	/// </summary>
	public interface IConfigurationRepository
	{
		/// <summary>
		/// Saves the given resource as local client configuration.
		/// </summary>
		void ConfigureClientWith(Uri resource, EncryptionParameters encryption = null);

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
		/// Registers the specified <see cref="IDataFormat"/> to be used when loading or saving configuration data.
		/// </summary>
		void Register(IDataFormat dataFormat);

		/// <summary>
		/// Registers the specified <see cref="IDataResource"/> to be used when loading or saving configuration data.
		/// </summary>
		void Register(IDataResource dataResource);

		/// <summary>
		/// Attempts to load settings from the specified resource.
		/// </summary>
		LoadStatus TryLoadSettings(Uri resource, PasswordParameters password, out EncryptionParameters encryption, out Format format, out Settings.Settings settings);

		/// <summary>
		/// Attempts to save settings according to the specified parameters.
		/// </summary>
		SaveStatus TrySaveSettings(Uri resource, Format format, Settings.Settings settings, EncryptionParameters encryption = null);
	}
}
