/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Contracts.Monitoring
{
	public delegate void WindowChangedHandler(IntPtr window, out bool hide);

	public interface IWindowMonitor
	{
		/// <summary>
		/// Event fired when the window monitor observes that the foreground window has changed.
		/// </summary>
		event WindowChangedHandler WindowChanged;

		/// <summary>
		/// Hides all currently opened windows.
		/// </summary>
		void HideAllWindows();

		/// <summary>
		/// Restores all windows which were hidden during the startup procedure.
		/// </summary>
		void RestoreHiddenWindows();

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
