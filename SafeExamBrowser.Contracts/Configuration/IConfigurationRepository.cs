/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Configuration.Settings;

namespace SafeExamBrowser.Contracts.Configuration
{
	public interface IConfigurationRepository
	{
		/// <summary>
		/// Retrieves the current settings, i.e. the last ones which were loaded. If no settings have been loaded yet, this property will
		/// be <c>null</c>!
		/// </summary>
		ISettings CurrentSettings { get; }

		/// <summary>
		/// The runtime information for the currently running application instance.
		/// </summary>
		IRuntimeInfo RuntimeInfo { get; }

		/// <summary>
		/// Attempts to load settings from the specified path.
		/// </summary>
		/// <exception cref="ArgumentException">Thrown if the given path cannot be resolved to a settings file.</exception>
		ISettings LoadSettings(Uri path);

		/// <summary>
		/// Loads the default settings.
		/// </summary>
		ISettings LoadDefaultSettings();
	}
}
