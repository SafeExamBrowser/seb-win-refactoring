/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Linq;
using System.Threading;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.WindowsApi;

namespace SafeExamBrowser.WindowsApi
{
	public class ExplorerShell : IExplorerShell
	{
		private ILogger logger;
		private INativeMethods nativeMethods;

		public ExplorerShell(ILogger logger, INativeMethods nativeMethods)
		{
			this.logger = logger;
			this.nativeMethods = nativeMethods;
		}

		public void Start()
		{
			var process = new System.Diagnostics.Process();
			var explorerPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");

			logger.Info("Restarting explorer shell...");

			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.FileName = explorerPath;
			process.Start();

			while (nativeMethods.GetShellWindowHandle() == IntPtr.Zero)
			{
				Thread.Sleep(20);
			}

			process.Refresh();
			logger.Info($"Explorer shell successfully started with PID = {process.Id}.");
			process.Close();
		}

		public void Terminate()
		{
			var processId = nativeMethods.GetShellProcessId();
			var explorerProcesses = System.Diagnostics.Process.GetProcessesByName("explorer");
			var shellProcess = explorerProcesses.FirstOrDefault(p => p.Id == processId);

			if (shellProcess != null)
			{
				logger.Debug($"Found explorer shell processes with PID = {processId}. Sending close message...");

				nativeMethods.PostCloseMessageToShell();

				while (!shellProcess.HasExited)
				{
					shellProcess.Refresh();
					Thread.Sleep(20);
				}

				logger.Info($"Successfully terminated explorer shell process with PID = {processId}.");
			}
			else
			{
				logger.Info("The explorer shell seems to already be terminated. Skipping this step...");
			}
		}
	}
}
