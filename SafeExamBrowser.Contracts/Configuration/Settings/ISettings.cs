/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
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
		/// The path of the application data folder.
		/// </summary>
		string AppDataFolder { get; }

		/// <summary>
		/// All browser-related settings.
		/// </summary>
		IBrowserSettings Browser { get; }

		/// <summary>
		/// All keyboard-related settings.
		/// </summary>
		IKeyboardSettings Keyboard { get; }

		/// <summary>
		/// All logging-related settings.
		/// </summary>
		ILoggingSettings Logging { get; }

		/// <summary>
		/// All mouse-related settings.
		/// </summary>
		IMouseSettings Mouse { get; }

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
		/// All taskbar-related settings.
		/// </summary>
		ITaskbarSettings Taskbar { get; }
	}
}
