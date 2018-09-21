/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.WindowsApi
{
	/// <summary>
	/// Defines an abstraction of the Windows explorer shell (i.e. the process controlling the GUI of the operating system).
	/// </summary>
	public interface IExplorerShell
	{
		/// <summary>
		/// Resumes the explorer shell process, if it was previously suspended.
		/// </summary>
		void Resume();

		/// <summary>
		/// Starts the Windows explorer shell, if it isn't already running.
		/// </summary>
		void Start();

		/// <summary>
		/// Suspends the explorer shell process, if it is running.
		/// </summary>
		void Suspend();

		/// <summary>
		/// Gracefully terminates the Windows explorer shell, if it is running.
		/// </summary>
		void Terminate();
	}
}
