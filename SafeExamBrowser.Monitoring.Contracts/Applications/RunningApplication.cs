/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
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
	public class RunningApplication
	{
		/// <summary>
		/// The name of the application.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// A list of processes which belong to the application.
		/// </summary>
		public IList<IProcess> Processes { get; }

		public RunningApplication(string name)
		{
			Name = name;
			Processes = new List<IProcess>();
		}
	}
}
