/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Monitoring.Contracts.System.Events;

namespace SafeExamBrowser.Monitoring.Contracts.System
{
	/// <summary>
	/// Provides the possibility to suppress and surveil various system components and functionalities.
	/// </summary>
	public interface ISystemSentinel
	{
		/// <summary>
		/// Event fired when the cursor configuration has changed.
		/// </summary>
		event SentinelEventHandler CursorChanged;

		/// <summary>
		/// Event fired when the ease of access configuration has changed.
		/// </summary>
		event SentinelEventHandler EaseOfAccessChanged;

		/// <summary>
		/// Event fired when the active user session has changed.
		/// </summary>
		event SessionChangedEventHandler SessionChanged;

		/// <summary>
		/// Event fired when the sticky keys state has changed.
		/// </summary>
		event SentinelEventHandler StickyKeysChanged;

		/// <summary>
		/// Attempts to disable the sticky keys. Returns <c>true</c> if successful, otherwise <c>false</c>.
		/// </summary>
		bool DisableStickyKeys();

		/// <summary>
		/// Attempts to enable the sticky keys. Returns <c>true</c> if successful, otherwise <c>false</c>.
		/// </summary>
		bool EnableStickyKeys();

		/// <summary>
		/// Attempts to revert the sticky keys to their initial state. Returns <c>true</c> if successful, otherwise <c>false</c>.
		/// </summary>
		bool RevertStickyKeys();

		/// <summary>
		/// Starts monitoring the cursor configuration.
		/// </summary>
		void StartMonitoringCursors();

		/// <summary>
		/// Starts monitoring the ease of access configuration.
		/// </summary>
		void StartMonitoringEaseOfAccess();

		/// <summary>
		/// Starts monitoring the sticky keys state.
		/// </summary>
		void StartMonitoringStickyKeys();

		/// <summary>
		/// Starts monitoring the system events.
		/// </summary>
		void StartMonitoringSystemEvents();

		/// <summary>
		/// Stops all monitoring operations.
		/// </summary>
		void StopMonitoring();

		/// <summary>
		/// Verifies the cursor configuration. Returns <c>true</c> if permitted, otherwise <c>false</c>.
		/// </summary>
		bool VerifyCursors();

		/// <summary>
		/// Verifies the ease of access configuration. Returns <c>true</c> if permitted, otherwise <c>false</c>.
		/// </summary>
		bool VerifyEaseOfAccess();
	}
}
