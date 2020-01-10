/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Settings.Browser
{
	/// <summary>
	/// Defines all settings for the browser engine.
	/// </summary>
	[Serializable]
	public class BrowserSettings
	{
		/// <summary>
		/// The settings to be used for additional browser windows.
		/// </summary>
		public WindowSettings AdditionalWindow { get; set; }

		/// <summary>
		/// Determines whether the user will be allowed to download configuration files.
		/// </summary>
		public bool AllowConfigurationDownloads { get; set; }

		/// <summary>
		/// Determines whether the user will be allowed to download files (excluding configuration files).
		/// </summary>
		public bool AllowDownloads { get; set; }

		/// <summary>
		/// Determines whether the user will be allowed to zoom webpages.
		/// </summary>
		public bool AllowPageZoom { get; set; }

		/// <summary>
		/// Determines whether the user needs to confirm the termination of SEB by <see cref="QuitUrl"/>.
		/// </summary>
		public bool ConfirmQuitUrl { get; set; }

		/// <summary>
		/// The custom user agent to optionally be used for all requests.
		/// </summary>
		public string CustomUserAgent { get; set; }

		/// <summary>
		/// Determines whether the user is allowed to use the integrated browser application.
		/// </summary>
		public bool EnableBrowser { get; set; }

		/// <summary>
		/// The settings to be used for the browser request filter.
		/// </summary>
		public FilterSettings Filter { get; set; }

		/// <summary>
		/// The hash value of the raw settings data, used for integrity checks with server applications (see also <see cref="SendCustomHeaders"/>).
		/// </summary>
		public string HashValue { get; set; }

		/// <summary>
		/// The settings to be used for the main browser window.
		/// </summary>
		public WindowSettings MainWindow { get; set; }

		/// <summary>
		/// Determines how attempts to open a popup are handled.
		/// </summary>
		public PopupPolicy PopupPolicy { get; set; }

		/// <summary>
		/// Determines the proxy settings to be used by the browser.
		/// </summary>
		public ProxySettings Proxy { get; set; }

		/// <summary>
		/// An URL which will initiate the termination of SEB if visited by the user.
		/// </summary>
		public string QuitUrl { get; set; }

		/// <summary>
		/// Determines whether custom request headers (e.g. for integrity checks) are sent with every HTTP request.
		/// </summary>
		public bool SendCustomHeaders { get; set; }

		/// <summary>
		/// The URL with which the main browser window will be loaded.
		/// </summary>
		public string StartUrl { get; set; }

		/// <summary>
		/// Determines whether a custom user agent will be used for all requests, see <see cref="CustomUserAgent"/>.
		/// </summary>
		public bool UseCustomUserAgent { get; set; }

		public BrowserSettings()
		{
			AdditionalWindow = new WindowSettings();
			Filter = new FilterSettings();
			MainWindow = new WindowSettings();
			Proxy = new ProxySettings();
		}
	}
}
