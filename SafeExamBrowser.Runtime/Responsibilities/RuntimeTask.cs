/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Runtime.Responsibilities
{
	/// <summary>
	/// Defines all tasks assumed by the responsibilities of the runtime application.
	/// </summary>
	internal enum RuntimeTask
	{
		/// <summary>
		/// Deregisters the respective event handlers during application termination.
		/// </summary>
		DeregisterEvents,

		/// <summary>
		/// Deregisters the respective event handlers during session termination.
		/// </summary>
		DeregisterSessionEvents,

		/// <summary>
		/// Registers the respective event handlers during application initialization.
		/// </summary>
		RegisterEvents,

		/// <summary>
		/// Registers the respective event handlers during session initialization.
		/// </summary>
		RegisterSessionEvents,

		/// <summary>
		/// Shows an error message in case the application shutdown fails.
		/// </summary>
		ShowShutdownError,

		/// <summary>
		/// Shows an error message in case the application startup fails.
		/// </summary>
		ShowStartupError,

		/// <summary>
		/// Attempts to start a new session.
		/// </summary>
		StartSession,

		/// <summary>
		/// Stops the currently running session.
		/// </summary>
		StopSession
	}
}
