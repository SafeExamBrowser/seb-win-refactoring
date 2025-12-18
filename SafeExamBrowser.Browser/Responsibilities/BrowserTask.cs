/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Browser.Responsibilities
{
	/// <summary>
	/// Defines all tasks assumed by the responsibilities of the <see cref="BrowserApplication"/>.
	/// </summary>
	internal enum BrowserTask
	{
		/// <summary>
		/// Closes all open windows during application termination.
		/// </summary>
		CloseAllWindows,

		/// <summary>
		/// Creates the main window during application initialization.
		/// </summary>
		CreateMainWindow,

		/// <summary>
		/// Deletes all browser cookies.
		/// </summary>
		DeleteCookies,

		/// <summary>
		/// Finalizes the browser cookies according to the active settings during application termination.
		/// </summary>
		FinalizeCookies,

		/// <summary>
		/// Initializes the browser cookies according to the active settings during application initialization.
		/// </summary>
		InitializeCookies,

		/// <summary>
		/// Initializes the file system (e.g. the down- and upload folder) during application initialization.
		/// </summary>
		InitializeFileSystem,

		/// <summary>
		/// Finalizes the file system (e.g. the down- and upload folder) during application termination.
		/// </summary>
		FinalizeFileSystem,

		/// <summary>
		/// Finalizes the browser cache according to the active settings during application termination.
		/// </summary>
		FinalizeCache,

		/// <summary>
		/// Initializes the integrity settings during application initialization.
		/// </summary>
		InitializeIntegrity,

		/// <summary>
		/// Initializes the configuration of the browser engine during application initialization.
		/// </summary>
		InitializeBrowserConfiguration,

		/// <summary>
		/// Initializes the browser preferences during application initialization.
		/// </summary>
		InitializePreferences,
	}
}
