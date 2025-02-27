/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Client.Contracts
{
	/// <summary>
	/// Coordinates concurrent operations of the client application.
	/// </summary>
	internal interface ICoordinator
	{
		/// <summary>
		/// Indicates whether the reconfiguration lock is currently occupied.
		/// </summary>
		bool IsReconfigurationLocked();

		/// <summary>
		/// Indicates whether the session lock is currently occupied.
		/// </summary>
		bool IsSessionLocked();

		/// <summary>
		/// Releases the reconfiguration lock.
		/// </summary>
		void ReleaseReconfigurationLock();

		/// <summary>
		/// Releases the session lock.
		/// </summary>
		void ReleaseSessionLock();

		/// <summary>
		/// Attempts to acquire the unique reconfiguration lock. Returns <c>true</c> if successful, otherwise <c>false</c>.
		/// </summary>
		bool RequestReconfigurationLock();

		/// <summary>
		/// Attempts to acquire the unique session lock. Returns <c>true</c> if successful, otherwise <c>false</c>.
		/// </summary>
		bool RequestSessionLock();
	}
}
