/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Monitoring.Contracts.Applications
{
	/// <summary>
	/// Provides information about a running application.
	/// </summary>
	public class RunningApplicationInfo
	{
		/// <summary>
		/// The name of the application.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// A list of processes which belong to the application.
		/// </summary>
		public IEnumerable<IProcess> Processes { get; set; }
	}
}
