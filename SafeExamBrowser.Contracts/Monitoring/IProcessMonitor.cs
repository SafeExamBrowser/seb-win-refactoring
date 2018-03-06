/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Contracts.Monitoring
{
	public delegate void ExplorerStartedEventHandler();

	/// <summary>
	/// Monitors the processes running on the computer and provides access to process-related functionality.
	/// </summary>
	public interface IProcessMonitor
	{
		/// <summary>
		/// Event fired when the process monitor observes that a new instance of
		/// the Windows explorer has been started.
		/// </summary>
		event ExplorerStartedEventHandler ExplorerStarted;

		/// <summary>
		/// Performs a check whether the process associated to the given window is allowed.
		/// </summary>
		bool BelongsToAllowedProcess(IntPtr window);

		/// <summary>
		/// Terminates the Windows explorer shell, i.e. the taskbar.
		/// </summary>
		void CloseExplorerShell();

		/// <summary>
		/// Starts a new instance of the Windows explorer shell.
		/// </summary>
		void StartExplorerShell();

		/// <summary>
		/// Starts monitoring the Windows explorer, i.e. any newly created instances of
		/// <c>explorer.exe</c> will trigger the <c>ExplorerStarted</c> event.
		/// </summary>
		void StartMonitoringExplorer();

		/// <summary>
		/// Stops monitoring the Windows explorer.
		/// </summary>
		void StopMonitoringExplorer();
	}
}
