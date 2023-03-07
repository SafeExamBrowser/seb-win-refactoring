/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;

namespace SafeExamBrowser.Settings.Applications
{
	/// <summary>
	/// Defines an application which is whitelisted, i.e. allowed to run during a session.
	/// </summary>
	[Serializable]
	public class WhitelistApplication
	{
		/// <summary>
		/// Determines whether the user may choose a custom path if the main executable cannot be found under <see cref="ExecutablePath"/>.
		/// </summary>
		public bool AllowCustomPath { get; set; }

		/// <summary>
		/// Determines whether the application may already be running when initializing a session. If <c>true</c>, <see cref="AutoTerminate"/> will be ignored.
		/// </summary>
		public bool AllowRunning { get; set; }

		/// <summary>
		/// The list of arguments to be used when starting the application.
		/// </summary>
		public IList<string> Arguments { get; }

		/// <summary>
		/// Determines whether the application will be automatically started when initializing a session.
		/// </summary>
		public bool AutoStart { get; set; }
		
		/// <summary>
		/// Specifies whether the application may be automatically terminated when starting a session. Is ignored if <see cref="AllowRunning"/> is set.
		/// </summary>
		public bool AutoTerminate { get; set; }

		/// <summary>
		/// Provides further information about the application.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// The display name to be used for the application (e.g. in the shell).
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// The file name of the main executable of the application.
		/// </summary>
		public string ExecutableName { get; set; }
		
		/// <summary>
		/// The path where the main executable of the application is located.
		/// </summary>
		public string ExecutablePath { get; set; }

		/// <summary>
		/// Unique identifier to be used to identify the application during runtime.
		/// </summary>
		public Guid Id { get; }

		/// <summary>
		/// The original file name of the main executable of the application, if available.
		/// </summary>
		public string OriginalName { get; set; }

		/// <summary>
		/// Determines whether the user will be able to access the application via the shell.
		/// </summary>
		public bool ShowInShell { get; set; }

		public WhitelistApplication()
		{
			Arguments = new List<string>();
			Id = Guid.NewGuid();
		}
	}
}
