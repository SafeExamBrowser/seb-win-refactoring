/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.WindowsApi;

namespace SafeExamBrowser.WindowsApi
{
	internal class Process : IProcess
	{
		private System.Diagnostics.Process process;

		public int Id
		{
			get { return process.Id; }
		}

		public Process(int id)
		{
			process = System.Diagnostics.Process.GetProcessById(id);
		}
	}
}
