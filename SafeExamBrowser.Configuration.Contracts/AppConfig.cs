/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Configuration.Contracts
{
	/// <summary>
	/// Defines the fundamental, global configuration information for all application components.
	/// </summary>
	[Serializable]
	public class AppConfig
	{
		/// <summary>
		/// The name of the backup data file used by the service component.
		/// </summary>
		public const string BACKUP_FILE_NAME = "Backup.bin";

		/// <summary>
		/// The base address for all communication hosts of the application.
		/// </summary>
		public const string BASE_ADDRESS = "net.pipe://localhost/safeexambrowser";

		/// <summary>
		/// The name of the synchronization primitive for the client component.
		/// </summary>
		public const string CLIENT_MUTEX_NAME = "safe_exam_browser_client_mutex";

		/// <summary>
		/// The name of the synchronization primitive for the runtime component.
		/// </summary>
		public const string RUNTIME_MUTEX_NAME = "safe_exam_browser_runtime_mutex";

		/// <summary>
		/// The communication address of the service component.
		/// </summary>
		public const string SERVICE_ADDRESS = BASE_ADDRESS + "/service";

		/// <summary>
		/// The name of the synchronization primitive for the service component.
		/// </summary>
		public const string SERVICE_MUTEX_NAME = "safe_exam_browser_reset_mutex";

		/// <summary>
		/// The file path of the local client configuration for the active user.
		/// </summary>
		public string AppDataFilePath { get; set; }

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
		public string BrowserLogFilePath { get; set; }

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
		public string ClientLogFilePath { get; set; }

		/// <summary>
		/// The hash value of the certificate used to sign the application binaries, or <c>null</c> if the binaries are unsigned.
		/// </summary>
		public string CodeSignatureHash { get; set; }

		/// <summary>
		/// The file extension of configuration files for the application (including the period).
		/// </summary>
		public string ConfigurationFileExtension { get; set; }

		/// <summary>
		/// The MIME type of configuration files for the application.
		/// </summary>
		public string ConfigurationFileMimeType { get; set; }

		/// <summary>
		/// The build version of the application.
		/// </summary>
		public string ProgramBuildVersion { get; set; }

		/// <summary>
		/// The copyright information for the application.
		/// </summary>
		public string ProgramCopyright { get; set; }

		/// <summary>
		/// The file path of the local client configuration for all users.
		/// </summary>
		public string ProgramDataFilePath { get; set; }

		/// <summary>
		/// The program title of the application.
		/// </summary>
		public string ProgramTitle { get; set; }

		/// <summary>
		/// The informational version of the application.
		/// </summary>
		public string ProgramInformationalVersion { get; set; }

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
		public string RuntimeLogFilePath { get; set; }

		/// <summary>
		/// The URI scheme for SEB resources.
		/// </summary>
		public string SebUriScheme { get; set; }

		/// <summary>
		/// The URI scheme for secure SEB resources.
		/// </summary>
		public string SebUriSchemeSecure { get; set; }

		/// <summary>
		/// The server API as JSON string.
		/// </summary>
		public string ServerApi { get; set; }

		/// <summary>
		/// The connection token for a server.
		/// </summary>
		public string ServerConnectionToken { get; set; }

		/// <summary>
		/// The identifier of the selected server exam.
		/// </summary>
		public string ServerExamId { get; set; }

		/// <summary>
		/// The OAuth2 token for a server.
		/// </summary>
		public string ServerOauth2Token { get; set; }

		/// <summary>
		/// The communication address of the service component.
		/// </summary>
		public string ServiceAddress { get; set; }

		/// <summary>
		/// The name of the global inter-process synchronization event hosted by the service.
		/// </summary>
		public string ServiceEventName { get; set; }

		/// <summary>
		/// The file path under which the log for the current session of the service component is to be stored.
		/// </summary>
		public string ServiceLogFilePath { get; set; }

		/// <summary>
		/// The file path under which the session cache is to be stored.
		/// </summary>
		public string SessionCacheFilePath { get; set; }

		/// <summary>
		/// The directory to be used for temporary application data.
		/// </summary>
		public string TemporaryDirectory { get; set; }

		/// <summary>
		/// Creates a shallow clone.
		/// </summary>
		public AppConfig Clone()
		{
			return MemberwiseClone() as AppConfig;
		}
	}
}
