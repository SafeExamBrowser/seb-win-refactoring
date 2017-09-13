/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.Configuration.Settings
{
	public interface ISettings
	{
		/// <summary>
		/// Determines whether the user may access the application log during runtime.
		/// </summary>
		bool AllowApplicationLog { get; }

		/// <summary>
		/// Determines whether the user may switch the keyboard layout during runtime.
		/// </summary>
		bool AllowKeyboardLayout { get; }

		/// <summary>
		/// The name used for the application data folder.
		/// </summary>
		string AppDataFolderName { get; }

		/// <summary>
		/// The file path under which the application log is to be stored.
		/// </summary>
		string ApplicationLogFile { get; }

		/// <summary>
		/// All browser-related settings.
		/// </summary>
		IBrowserSettings Browser { get; }

		/// <summary>
		/// All keyboard-related settings.
		/// </summary>
		IKeyboardSettings Keyboard { get; }

		/// <summary>
		/// All mouse-related settings.
		/// </summary>
		IMouseSettings Mouse { get; }

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

		/// <summary>
		/// A string uniquely identifying the runtime of the application, used e.g. for the log file names.
		/// </summary>
		string RuntimeIdentifier { get; }
	}
}
