/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.Monitoring
{
	public interface IProcessMonitor
	{
		/// <summary>
		/// Starts a new instance of the Windows explorer shell.
		/// </summary>
		void StartExplorerShell();

		/// <summary>
		/// Starts monitoring the Windows explorer, i.e. any newly created instances of
		/// <c>explorer.exe</c> will automatically be terminated.
		/// </summary>
		void StartMonitoringExplorer();

		/// <summary>
		/// Stops monitoring the Windows explorer.
		/// </summary>
		void StopMonitoringExplorer();

		/// <summary>
		/// Terminates the Windows explorer shell, i.e. the taskbar.
		/// </summary>
		void CloseExplorerShell();
	}
}
