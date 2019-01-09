/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Monitoring.Events;

namespace SafeExamBrowser.Contracts.Monitoring
{
	/// <summary>
	/// Monitors the windows associated with the current desktop and provides window-related functionality.
	/// </summary>
	public interface IWindowMonitor
	{
		/// <summary>
		/// Event fired when the window monitor observes that the foreground window has changed.
		/// </summary>
		event WindowChangedEventHandler WindowChanged;

		/// <summary>
		/// Forcefully closes the specified window.
		/// </summary>
		void Close(IntPtr window);

		/// <summary>
		/// Hides the specified window. Returns <c>true</c> if the window was successfully hidden, otherwise <c>false</c>.
		/// </summary>
		bool Hide(IntPtr window);

		/// <summary>
		/// Starts monitoring application windows by subscribing to specific system events.
		/// If a window is shown which is not supposed to do so, it will be automatically hidden.
		/// </summary>
		void StartMonitoringWindows();

		/// <summary>
		/// Stops monitoring windows and deregisters from any subscribed system events.
		/// </summary>
		void StopMonitoringWindows();
	}
}
