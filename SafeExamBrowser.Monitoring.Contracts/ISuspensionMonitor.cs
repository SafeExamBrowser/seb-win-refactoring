/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Monitoring.Contracts.Events;

namespace SafeExamBrowser.Monitoring.Contracts
{
	/// <summary>
	/// Monitors the execution state of the application for suspensions.
	/// </summary>
	public interface ISuspensionMonitor
	{
		/// <summary>
		/// Fired when a partial or complete suspension of the application has been detected.
		/// </summary>
		event SuspensionDetectedEventHandler SuspensionDetected;

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
