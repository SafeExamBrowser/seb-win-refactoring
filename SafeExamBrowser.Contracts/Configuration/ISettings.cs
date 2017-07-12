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
		/// The copyright information for the application, to be displayed in e.g. the log or the splash screen.
		/// </summary>
		string CopyrightInfo { get; }

		/// <summary>
		/// The path where the log files are to be stored.
		/// </summary>
		string LogFolderPath { get; }

		/// <summary>
		/// The information to be printed at the beginning of the application log.
		/// </summary>
		string LogHeader { get; }

		/// <summary>
		/// The program version of the application.
		/// </summary>
		string ProgramVersion { get; }
	}
}
