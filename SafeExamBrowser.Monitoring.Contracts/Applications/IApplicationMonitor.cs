/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Monitoring.Contracts.Applications.Events;
using SafeExamBrowser.Settings.Applications;

namespace SafeExamBrowser.Monitoring.Contracts.Applications
{
	/// <summary>
	/// Monitors applications running on the computer.
	/// </summary>
	public interface IApplicationMonitor
	{
		/// <summary>
		/// Event fired when a new instance of the Windows Explorer has been started.
		/// </summary>
		event ExplorerStartedEventHandler ExplorerStarted;

		/// <summary>
		/// Event fired when a new instance of a whitelisted application has been started.
		/// </summary>
		event InstanceStartedEventHandler InstanceStarted;

		/// <summary>
		/// Event fired when the automatic termination of a blacklisted application failed.
		/// </summary>
		event TerminationFailedEventHandler TerminationFailed;

		/// <summary>
		/// Initializes the application monitor.
		/// </summary>
		InitializationResult Initialize(ApplicationSettings settings);

		/// <summary>
		/// Starts monitoring all initialized applications. Windows Explorer will always be monitored.
		/// </summary>
		void Start();

		/// <summary>
		/// Stops the application monitoring.
		/// </summary>
		void Stop();

		/// <summary>
		/// Attempts to terminate all processes of the specified application. Returns <c>true</c> if successful, otherwise <c>false</c>.
		/// </summary>
		bool TryTerminate(RunningApplication application);
	}
}
