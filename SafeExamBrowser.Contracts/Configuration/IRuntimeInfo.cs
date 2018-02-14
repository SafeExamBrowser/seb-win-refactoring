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
	public interface IRuntimeInfo
	{
		/// <summary>
		/// The path of the application data folder.
		/// </summary>
		string AppDataFolder { get; }

		/// <summary>
		/// The point in time when the application was started.
		/// </summary>
		DateTime ApplicationStartTime { get; }

		/// <summary>
		/// The path where the browser cache is to be stored.
		/// </summary>
		string BrowserCachePath { get; }

		/// <summary>
		/// The file path under which the log of the browser component is to be stored.
		/// </summary>
		string BrowserLogFile { get; }

		/// <summary>
		/// The communication address of the client component.
		/// 
		/// TODO: Will need to be updated for each new client instance!
		/// 
		/// </summary>
		string ClientAddress { get; }

		/// <summary>
		/// The executable path of the client compontent.
		/// </summary>
		string ClientExecutablePath { get; }

		/// <summary>
		/// The unique identifier for the currently running client instance.
		/// 
		/// TODO: Will need to be updated for each new client instance! -> Remove if unused!
		/// 
		/// </summary>
		Guid ClientId { get; }

		/// <summary>
		/// The file path under which the log of the client component is to be stored.
		/// </summary>
		string ClientLogFile { get; }

		/// <summary>
		/// The default file name for application settings.
		/// </summary>
		string DefaultSettingsFileName { get; }

		/// <summary>
		/// The copyright information for the application (i.e. the executing assembly).
		/// </summary>
		string ProgramCopyright { get; }

		/// <summary>
		/// The path of the program data folder.
		/// </summary>
		string ProgramDataFolder { get; }

		/// <summary>
		/// The program title of the application (i.e. the executing assembly).
		/// </summary>
		string ProgramTitle { get; }

		/// <summary>
		/// The program version of the application (i.e. the executing assembly).
		/// </summary>
		string ProgramVersion { get; }

		/// <summary>
		/// The communication address of the runtime component.
		/// </summary>
		string RuntimeAddress { get; }

		/// <summary>
		/// The unique identifier for the currently running runtime instance.
		/// 
		/// TODO: Remove if unused!
		/// 
		/// </summary>
		Guid RuntimeId { get; }

		/// <summary>
		/// The file path under which the log of the runtime component is to be stored.
		/// </summary>
		string RuntimeLogFile { get; }

		/// <summary>
		/// The communication address of the service component.
		/// </summary>
		string ServiceAddress { get; }
	}
}
