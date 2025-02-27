/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Monitoring.Contracts.System.Events
{
	/// <summary>
	/// The event arguments used for the <see cref="SentinelEventHandler"/> fired by the <see cref="ISystemSentinel"/>.
	/// </summary>
	public class SentinelEventArgs
	{
		/// <summary>
		/// Indicates whether a configuration or state change shall be allowed.
		/// </summary>
		public bool Allow { get; set; }
	}
}
