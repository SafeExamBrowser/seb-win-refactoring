/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		private IList<ProcessThread> suspendedThreads;

		public ExplorerShell(ILogger logger, INativeMethods nativeMethods)
		{
			this.logger = logger;
			this.nativeMethods = nativeMethods;
			this.suspendedThreads = new List<ProcessThread>();
		}

		public void Resume()
		{
			const int MAX_ATTEMPTS = 3;

			logger.Debug($"Attempting to resume all {suspendedThreads.Count} previously suspended explorer shell threads...");

			for (var attempts = 0; suspendedThreads.Any(); attempts++)
			{
				var thread = suspendedThreads.First();
				var success = nativeMethods.ResumeThread(thread.Id);

				if (success || attempts == MAX_ATTEMPTS)
				{
					attempts = 0;
					suspendedThreads.Remove(thread);

					if (success)
					{
						logger.Debug($"Successfully resumed thread #{thread.Id} of explorer shell process.");
					}
					else
					{
						logger.Warn($"Failed to resume thread #{thread.Id} of explorer shell process within {MAX_ATTEMPTS} attempts!");
					}
				}
			}

			logger.Info($"Successfully resumed explorer shell process.");
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

		public void Suspend()
		{
			var processId = nativeMethods.GetShellProcessId();
			var explorerProcesses = System.Diagnostics.Process.GetProcessesByName("explorer");
			var shellProcess = explorerProcesses.FirstOrDefault(p => p.Id == processId);

			if (shellProcess != null)
			{
				logger.Debug($"Found explorer shell processes with PID = {processId}.");

				foreach (ProcessThread thread in shellProcess.Threads)
				{
					var success = nativeMethods.SuspendThread(thread.Id);

					if (success)
					{
						suspendedThreads.Add(thread);
						logger.Debug($"Successfully suspended thread #{thread.Id} of explorer shell process.");
					}
					else
					{
						logger.Warn($"Failed to suspend thread #{thread.Id} of explorer shell process!");
					}
				}

				logger.Info($"Successfully suspended explorer shell process with PID = {processId}.");
			}
			else
			{
				logger.Info("The explorer shell can't be suspended, as it seems to not be running.");
			}
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
				logger.Info("The explorer shell seems to already be terminated.");
			}
		}
	}
}
