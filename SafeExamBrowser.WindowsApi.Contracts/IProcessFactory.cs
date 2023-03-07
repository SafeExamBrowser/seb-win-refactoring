/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;

namespace SafeExamBrowser.WindowsApi.Contracts
{
	/// <summary>
	/// The factory for processes, to be used whenever a new process needs to be created (for internal components and external applications).
	/// </summary>
	public interface IProcessFactory
	{
		/// <summary>
		/// Allows to define the desktop on which new processes should be started. If no startup desktop is specified, processes will be
		/// started on the same desktop which was active when the application itself was started.
		/// </summary>
		IDesktop StartupDesktop { set; }

		/// <summary>
		/// Retrieves all currently running processes.
		/// </summary>
		IList<IProcess> GetAllRunning();

		/// <summary>
		/// Starts a new process with the given command-line arguments.
		/// </summary>
		/// <exception cref="System.ComponentModel.Win32Exception">If the process could not be started.</exception>
		IProcess StartNew(string path, params string[] args);

		/// <summary>
		/// Attempts to retrieve a process by its identifier. Returns <c>true</c> if a process was found, otherwise <c>false</c>.
		/// </summary>
		bool TryGetById(int id, out IProcess process);
	}
}
