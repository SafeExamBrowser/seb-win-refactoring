/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.Configuration
{
	public interface ISettings
	{
		/// <summary>
		/// The file path under which the application log is to be stored.
		/// </summary>
		string ApplicationLogFile { get; }

		/// <summary>
		/// The path where the browser cache is to be stored.
		/// </summary>
		string BrowserCachePath { get; }

		/// <summary>
		/// The file path under which the browser log is to be stored.
		/// </summary>
		string BrowserLogFile { get; }

		/// <summary>
		/// The path where the log files are to be stored.
		/// </summary>
		string LogFolderPath { get; }

		/// <summary>
		/// The copyright information for the application (i.e. the executing assembly).
		/// </summary>
		string ProgramCopyright { get; }

		/// <summary>
		/// The program title of the application (i.e. the executing assembly).
		/// </summary>
		string ProgramTitle { get; }

		/// <summary>
		/// The program version of the application (i.e. the executing assembly).
		/// </summary>
		string ProgramVersion { get; }
	}
}
