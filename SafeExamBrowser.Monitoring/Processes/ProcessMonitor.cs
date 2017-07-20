/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.WindowsApi;

namespace SafeExamBrowser.Monitoring.Processes
{
	public class ProcessMonitor : IProcessMonitor
	{
		private ILogger logger;

		public ProcessMonitor(ILogger logger)
		{
			this.logger = logger;
		}

		public void StartExplorerShell()
		{
			var process = new Process();
			var explorerPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");

			logger.Info("Restarting explorer shell...");

			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.FileName = explorerPath;
			process.Start();

			while (User32.GetShellWindowHandle() == IntPtr.Zero)
			{
				Thread.Sleep(20);
			}

			process.Refresh();
			logger.Info($"Explorer shell successfully started with PID = {process.Id}.");
			process.Close();
		}

		public void StartMonitoringExplorer()
		{
			// TODO
		}

		public void StopMonitoringExplorer()
		{
			// TODO
		}

		public void CloseExplorerShell()
		{
			var processId = User32.GetShellProcessId();
			var explorerProcesses = Process.GetProcessesByName("explorer");
			var shellProcess = explorerProcesses.FirstOrDefault(p => p.Id == processId);

			if (shellProcess != null)
			{
				logger.Info($"Found explorer shell processes with PID = {processId}. Sending close message...");
				User32.PostCloseMessageToShell();

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
