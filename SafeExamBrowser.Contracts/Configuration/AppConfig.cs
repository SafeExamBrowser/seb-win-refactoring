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
	/// Defines the fundamental, global configuration information for all application components.
	/// </summary>
	[Serializable]
	public class AppConfig
	{
		/// <summary>
		/// The path of the application data folder.
		/// </summary>
		public string AppDataFolder { get; set; }

		/// <summary>
		/// The point in time when the application was started.
		/// </summary>
		public DateTime ApplicationStartTime { get; set; }

		/// <summary>
		/// The path where the browser cache is to be stored.
		/// </summary>
		public string BrowserCachePath { get; set; }

		/// <summary>
		/// The file path under which the log of the browser component is to be stored.
		/// </summary>
		public string BrowserLogFile { get; set; }

		/// <summary>
		/// The communication address of the client component.
		/// </summary>
		public string ClientAddress { get; set; }

		/// <summary>
		/// The executable path of the client compontent.
		/// </summary>
		public string ClientExecutablePath { get; set; }

		/// <summary>
		/// The unique identifier for the currently running client instance.
		/// </summary>
		public Guid ClientId { get; set; }

		/// <summary>
		/// The file path under which the log of the client component is to be stored.
		/// </summary>
		public string ClientLogFile { get; set; }

		/// <summary>
		/// The file extension of configuration files for the application (including the period).
		/// </summary>
		public string ConfigurationFileExtension { get; set; }

		/// <summary>
		/// The default file name for application settings.
		/// </summary>
		public string DefaultSettingsFileName { get; set; }

		/// <summary>
		/// The default directory for file downloads.
		/// </summary>
		public string DownloadDirectory { get; set; }

		/// <summary>
		/// The copyright information for the application (i.e. the executing assembly).
		/// </summary>
		public string ProgramCopyright { get; set; }

		/// <summary>
		/// The path of the program data folder.
		/// </summary>
		public string ProgramDataFolder { get; set; }

		/// <summary>
		/// The program title of the application (i.e. the executing assembly).
		/// </summary>
		public string ProgramTitle { get; set; }

		/// <summary>
		/// The program version of the application (i.e. the executing assembly).
		/// </summary>
		public string ProgramVersion { get; set; }

		/// <summary>
		/// The communication address of the runtime component.
		/// </summary>
		public string RuntimeAddress { get; set; }

		/// <summary>
		/// The unique identifier for the currently running runtime instance.
		/// </summary>
		public Guid RuntimeId { get; set; }

		/// <summary>
		/// The file path under which the log of the runtime component is to be stored.
		/// </summary>
		public string RuntimeLogFile { get; set; }

		/// <summary>
		/// The communication address of the service component.
		/// </summary>
		public string ServiceAddress { get; set; }
	}
}
