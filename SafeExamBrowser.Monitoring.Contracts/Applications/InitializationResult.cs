/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;

namespace SafeExamBrowser.Monitoring.Contracts.Applications
{
	/// <summary>
	/// Provides information about the initialization of the <see cref="IApplicationMonitor"/>.
	/// </summary>
	public class InitializationResult
	{
		/// <summary>
		/// A list of currently running applications which could not be automatically terminated.
		/// </summary>
		public IList<RunningApplication> FailedAutoTerminations { get; }

		/// <summary>
		/// A list of currently running applications which need to be terminated.
		/// </summary>
		public IList<RunningApplication> RunningApplications { get; }

		public InitializationResult()
		{
			FailedAutoTerminations = new List<RunningApplication>();
			RunningApplications = new List<RunningApplication>();
		}
	}
}
