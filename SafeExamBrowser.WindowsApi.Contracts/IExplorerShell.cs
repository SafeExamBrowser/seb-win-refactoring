/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.WindowsApi.Contracts
{
	/// <summary>
	/// Defines an abstraction of the Windows explorer shell (i.e. the process controlling the GUI of the operating system).
	/// </summary>
	public interface IExplorerShell
	{
		/// <summary>
		/// Hides all currently opened windows. The explorer shell needs to be running in order to execute this operation!
		/// </summary>
		void HideAllWindows();

		/// <summary>
		/// Restores all previously hidden windows. The explorer shell needs to be running in order to execute this operation!
		/// </summary>
		void RestoreAllWindows();

		/// <summary>
		/// Starts the Windows explorer shell, if it isn't already running.
		/// </summary>
		void Start();

		/// <summary>
		/// Gracefully terminates the Windows explorer shell, if it is running.
		/// </summary>
		void Terminate();
	}
}
