/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Client.Responsibilities
{
	/// <summary>
	/// Defines all tasks assumed by the responsibilities of the client application.
	/// </summary>
	internal enum ClientTask
	{
		/// <summary>
		/// Auto-start the browser and any potential third-party applications.
		/// </summary>
		AutoStartApplications,

		/// <summary>
		/// Close the shell.
		/// </summary>
		CloseShell,

		/// <summary>
		/// Deregister all event handlers during application termination.
		/// </summary>
		DeregisterEvents,

		/// <summary>
		/// Execute wave 1 of the application shutdown preparation. It should be used for potentially long-running operations which require all
		/// other (security) functionalities to continue working normally.
		/// </summary>
		PrepareShutdown_Wave1,

		/// <summary>
		/// Execute wave 2 of the application shutdown preparation. It should be used by all remaining responsibilities which must continue to work
		/// normally during the execution of wave 1.
		/// </summary>
		PrepareShutdown_Wave2,

		/// <summary>
		/// Register all event handlers during application initialization.
		/// </summary>
		RegisterEvents,

		/// <summary>
		/// Schedule the verification of the application integrity.
		/// </summary>
		ScheduleIntegrityVerification,

		/// <summary>
		/// Show the shell.
		/// </summary>
		ShowShell,

		/// <summary>
		/// Start the monitoring of different (security) aspects.
		/// </summary>
		StartMonitoring,

		/// <summary>
		/// Update the session integrity during application termination.
		/// </summary>
		UpdateSessionIntegrity,

		/// <summary>
		/// Verify the session integrity during application initialization.
		/// </summary>
		VerifySessionIntegrity
	}
}
