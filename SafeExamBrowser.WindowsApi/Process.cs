/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.WindowsApi.Contracts;
using SafeExamBrowser.WindowsApi.Contracts.Events;

namespace SafeExamBrowser.WindowsApi
{
	internal class Process : IProcess
	{
		private System.Diagnostics.Process process;

		public event ProcessTerminatedEventHandler Terminated;

		public int Id
		{
			get { return process.Id; }
		}

		public bool HasTerminated
		{
			get { process.Refresh(); return process.HasExited; }
		}

		public Process(int id)
		{
			process = System.Diagnostics.Process.GetProcessById(id);
			process.Exited += Process_Exited;
			process.EnableRaisingEvents = true;
		}

		public void Kill()
		{
			process.Refresh();

			if (!process.HasExited)
			{
				process.Kill();
			}
		}

		private void Process_Exited(object sender, System.EventArgs e)
		{
			Terminated?.Invoke(process.ExitCode);
		}
	}
}
