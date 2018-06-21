/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Contracts.Configuration.Settings
{
	/// <summary>
	/// Defines all configuration options for the browser of the application.
	/// </summary>
	[Serializable]
	public class BrowserSettings
	{
		/// <summary>
		/// Determines whether the user should be allowed to change the URL of a browser window.
		/// </summary>
		public bool AllowAddressBar { get; set; }

		/// <summary>
		/// Determines whether the user should be allowed to navigate backwards in a browser window.
		/// </summary>
		public bool AllowBackwardNavigation { get; set; }

		/// <summary>
		/// Determines whether the user should be allowed to open the developer console of a browser window.
		/// </summary>
		public bool AllowDeveloperConsole { get; set; }

		/// <summary>
		/// Determines whether the user should be allowed to download files.
		/// </summary>
		public bool AllowDownloads { get; set; }

		/// <summary>
		/// Determines whether the user should be allowed to navigate forwards in a browser window.
		/// </summary>
		public bool AllowForwardNavigation { get; set; }

		/// <summary>
		/// Determines whether the user should be allowed to reload webpages.
		/// </summary>
		public bool AllowReloading { get; set; }

		/// <summary>
		/// Determines whether the main browser window should be rendered in fullscreen mode, i.e. without window frame.
		/// </summary>
		public bool FullScreenMode { get; set; }

		/// <summary>
		/// The start URL with which a new browser window should be loaded.
		/// </summary>
		public string StartUrl { get; set; }
	}
}
