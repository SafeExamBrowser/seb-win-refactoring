/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Proctoring.Contracts.Events
{
	/// <summary>
	/// The event arguments used for the remaining work updated event fired by the <see cref="IProctoringController"/>.
	/// </summary>
	public class RemainingWorkUpdatedEventArgs
	{
		/// <summary>
		/// Determines whether the execution of the remaining work may be cancelled.
		/// </summary>
		public bool AllowCancellation { get; set; }

		/// <summary>
		/// The path of the local cache, if <see cref="HasFailed"/> is <c>true</c>.
		/// </summary>
		public string CachePath { get; set; }

		/// <summary>
		/// Indicates that the cancellation of the remaining work has been requested.
		/// </summary>
		public bool CancellationRequested { get; set; }

		/// <summary>
		/// Indicates that the execution of the remaining work has failed.
		/// </summary>
		public bool HasFailed { get; set; }

		/// <summary>
		/// Indicates that the execution of the remaining work has finished.
		/// </summary>
		public bool IsFinished { get; set; }

		/// <summary>
		/// Indicates that the execution is paused resp. waiting to be resumed.
		/// </summary>
		public bool IsWaiting { get; set; }

		/// <summary>
		/// The point in time when the next transmission will take place, if available.
		/// </summary>
		public DateTime? Next { get; set; }

		/// <summary>
		/// The number of already executed work items.
		/// </summary>
		public int Progress { get; set; }

		/// <summary>
		/// The point in time when the execution will resume, if available.
		/// </summary>
		public DateTime? Resume { get; set; }

		/// <summary>
		/// The total number of work items to be executed.
		/// </summary>
		public int Total { get; set; }
	}
}
