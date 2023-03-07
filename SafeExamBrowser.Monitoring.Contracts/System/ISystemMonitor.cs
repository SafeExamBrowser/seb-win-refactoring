/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Monitoring.Contracts.System.Events;

namespace SafeExamBrowser.Monitoring.Contracts.System
{
	/// <summary>
	/// Monitors the operating system, e.g. for user account related events.
	/// </summary>
	public interface ISystemMonitor
	{
		/// <summary>
		/// Event fired when the active user session has changed.
		/// </summary>
		event SessionSwitchedEventHandler SessionSwitched;

		/// <summary>
		/// Starts the monitoring.
		/// </summary>
		void Start();

		/// <summary>
		/// Stops the monitoring.
		/// </summary>
		void Stop();
	}
}
