/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Threading;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Lockdown.Contracts;

namespace SafeExamBrowser.Service
{
	/// <summary>
	/// Holds all configuration and runtime data required for the session handling.
	/// </summary>
	internal class SessionContext
	{
		/// <summary>
		/// The mechanism trying to restore all changes after an application or system failure.
		/// </summary>
		internal IAutoRestoreMechanism AutoRestoreMechanism { get; set; }

		/// <summary>
		/// The configuration of the currently active session.
		/// </summary>
		internal ServiceConfiguration Configuration { get; set; }

		/// <summary>
		/// Indicates whether a session is currently running.
		/// </summary>
		internal bool IsRunning { get; set; }

		/// <summary>
		/// The global inter-process event used for status synchronization with the runtime component.
		/// </summary>
		internal EventWaitHandle ServiceEvent { get; set; }
	}
}
