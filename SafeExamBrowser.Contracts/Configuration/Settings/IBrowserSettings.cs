/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.Configuration.Settings
{
	public interface IBrowserSettings
	{
		/// <summary>
		/// Determines whether the user should be allowed to change the URL of a browser window.
		/// </summary>
		bool AllowAddressBar { get; }

		/// <summary>
		/// Determines whether the user should be allowed to navigate backwards in a browser window.
		/// </summary>
		bool AllowBackwardNavigation { get; }

		/// <summary>
		/// Determines whether the user should be allowed to open the developer console of a browser window.
		/// </summary>
		bool AllowDeveloperConsole { get; }

		/// <summary>
		/// Determines whether the user should be allowed to navigate forwards in a browser window.
		/// </summary>
		bool AllowForwardNavigation { get; }

		/// <summary>
		/// Determines whether the user should be allowed to reload webpages.
		/// </summary>
		bool AllowReloading { get; }

		/// <summary>
		/// Determines whether the main browser window should be rendered in fullscreen mode, i.e. without window frame.
		/// </summary>
		bool FullScreenMode { get; }

		/// <summary>
		/// The start URL with which a new browser window should be loaded.
		/// </summary>
		string StartUrl { get; }
	}
}
