/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Settings.Browser
{
	/// <summary>
	/// Defines all settings for the integrated browser application.
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
		/// Determines whether the user will be allowed to select a custom location when down- or uploading a file (excluding configuration files).
		/// </summary>
		public bool AllowCustomDownAndUploadLocation { get; set; }

		/// <summary>
		/// Determines whether the user will be allowed to download files (excluding configuration files).
		/// </summary>
		public bool AllowDownloads { get; set; }

		/// <summary>
		/// Determines whether the user will be allowed to search page contents.
		/// </summary>
		public bool AllowFind { get; set; }

		/// <summary>
		/// Determines whether the user will be allowed to zoom webpages.
		/// </summary>
		public bool AllowPageZoom { get; set; }

		/// <summary>
		/// Determines whether the internal PDF reader of the browser application is enabled. If not, documents will be downloaded by default.
		/// </summary>
		public bool AllowPdfReader { get; set; }

		/// <summary>
		/// Determines whether the toolbar of the internal PDF reader (which allows to e.g. download or print a document) will be enabled.
		/// </summary>
		public bool AllowPdfReaderToolbar { get; set; }

		/// <summary>
		/// Determines whether the user will be allowed to print web content. To control printing in PDF documents, see <see cref="AllowPdfReaderToolbar"/>.
		/// </summary>
		public bool AllowPrint { get; set; }

		/// <summary>
		/// Determines whether spell checking is enabled for input fields.
		/// </summary>
		public bool AllowSpellChecking { get; set; }

		/// <summary>
		/// Determines whether the user will be allowed to upload files.
		/// </summary>
		public bool AllowUploads { get; set; }

		/// <summary>
		/// The salt value for the calculation of the browser exam key which is used for integrity checks with server applications (see also <see cref="SendBrowserExamKey"/>).
		/// </summary>
		public byte[] BrowserExamKeySalt { get; set; }

		/// <summary>
		/// The configuration key used for integrity checks with server applications (see also <see cref="SendConfigurationKey"/>).
		/// </summary>
		public string ConfigurationKey { get; set; }

		/// <summary>
		/// Determines whether the user needs to confirm the termination of SEB by <see cref="QuitUrl"/>.
		/// </summary>
		public bool ConfirmQuitUrl { get; set; }

		/// <summary>
		/// An optional, custom browser exam key used for integrity checks with server applications (see also <see cref="SendBrowserExamKey"/>).
		/// </summary>
		public string CustomBrowserExamKey { get; set; }

		/// <summary>
		/// The custom user agent to optionally be used for all requests.
		/// </summary>
		public string CustomUserAgent { get; set; }

		/// <summary>
		/// Determines whether the entire browser cache is deleted when terminating the application. IMPORTANT: If <see cref="DeleteCookiesOnShutdown"/>
		/// is set to <c>false</c>, the cache will not be deleted in order to keep the cookies for the next session.
		/// </summary>
		public bool DeleteCacheOnShutdown { get; set; }

		/// <summary>
		/// Determines whether all cookies are deleted when terminating the browser application. IMPORTANT: The browser cache will not be deleted
		/// if set to <c>false</c>, even if <see cref="DeleteCacheOnShutdown"/> is set to <c>true</c>!
		/// </summary>
		public bool DeleteCookiesOnShutdown { get; set; }

		/// <summary>
		/// Determines whether all cookies are deleted when starting the browser application.
		/// </summary>
		public bool DeleteCookiesOnStartup { get; set; }

		/// <summary>
		/// Defines a custom directory for down- and uploads. If not defined, all operations will be directed to the current user's download directory.
		/// </summary>
		public string DownAndUploadDirectory { get; set; }

		/// <summary>
		/// Determines whether the user is allowed to use the integrated browser application.
		/// </summary>
		public bool EnableBrowser { get; set; }

		/// <summary>
		/// The settings to be used for the browser request filter.
		/// </summary>
		public FilterSettings Filter { get; set; }

		/// <summary>
		/// An optional custom message shown before navigating home.
		/// </summary>
		public string HomeNavigationMessage { get; set; }

		/// <summary>
		/// Determines whether a password is required to navigate home.
		/// </summary>
		public bool HomeNavigationRequiresPassword { get; set; }

		/// <summary>
		/// The hash code of the password optionally required to navigate home.
		/// </summary>
		public string HomePasswordHash { get; set; }

		/// <summary>
		/// An optional custom URL to be used when navigating home.
		/// </summary>
		public string HomeUrl { get; set; }

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
		/// An URL which will initiate the termination of SEB (or reset the browser if <see cref="ResetOnQuitUrl"/> is <c>true</c>) when visited by the user.
		/// </summary>
		public string QuitUrl { get; set; }

		/// <summary>
		/// Determines whether the browser should be reset when a <see cref="QuitUrl"/> is detected.
		/// </summary>
		public bool ResetOnQuitUrl { get; set; }

		/// <summary>
		/// Determines whether the configuration key header is sent with every HTTP request (see also <see cref="ConfigurationKey"/>).
		/// </summary>
		public bool SendConfigurationKey { get; set; }

		/// <summary>
		/// Determines whether the browser exam key header is sent with every HTTP request (see also <see cref="BrowserExamKeySalt"/> and <see cref="CustomBrowserExamKey"/>).
		/// </summary>
		public bool SendBrowserExamKey { get; set; }

		/// <summary>
		/// Determines whether the user will be able to see the path of a file system element in the file system dialog (e.g. when down- or uploading a file).
		/// </summary>
		public bool ShowFileSystemElementPath { get; set; }

		/// <summary>
		/// The URL with which the main browser window will be loaded.
		/// </summary>
		public string StartUrl { get; set; }

		/// <summary>
		/// A query for the <see cref="StartUrl"/> which SEB automatically extracts from the configuration URL.
		/// </summary>
		public string StartUrlQuery { get; set; }

		/// <summary>
		/// Determines whether a custom user agent will be used for all requests, see <see cref="CustomUserAgent"/>.
		/// </summary>
		public bool UseCustomUserAgent { get; set; }

		/// <summary>
		/// Determines whether the <see cref="StartUrlQuery"/> will be appended to the <see cref="StartUrl"/>.
		/// </summary>
		public bool UseQueryParameter { get; set; }

		/// <summary>
		/// A custom suffix to be appended to the user agent.
		/// </summary>
		public string UserAgentSuffix { get; set; }

		/// <summary>
		/// Determines whether the start URL will be used when navigating home.
		/// </summary>
		public bool UseStartUrlAsHomeUrl { get; set; }

		/// <summary>
		/// Determines whether a temporary directory should be used for down- and uploads.
		/// </summary>
		public bool UseTemporaryDownAndUploadDirectory { get; set; }

		public BrowserSettings()
		{
			AdditionalWindow = new WindowSettings();
			Filter = new FilterSettings();
			MainWindow = new WindowSettings();
			Proxy = new ProxySettings();
		}
	}
}
