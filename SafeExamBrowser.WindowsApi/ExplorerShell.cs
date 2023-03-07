/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
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
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.WindowsApi.Contracts;
using SafeExamBrowser.WindowsApi.Types;

namespace SafeExamBrowser.WindowsApi
{
	public class ExplorerShell : IExplorerShell
	{
		private ILogger logger;
		private INativeMethods nativeMethods;
		private IList<Window> minimizedWindows = new List<Window>();

		public ExplorerShell(ILogger logger, INativeMethods nativeMethods)
		{
			this.logger = logger;
			this.nativeMethods = nativeMethods;
			this.minimizedWindows = new List<Window>();
		}

		public void HideAllWindows()
		{
			logger.Info("Searching for windows to be minimized...");

			foreach (var handle in nativeMethods.GetOpenWindows())
			{
				var window = new Window
				{
					Handle = handle,
					Title = nativeMethods.GetWindowTitle(handle)
				};

				minimizedWindows.Add(window);
				logger.Info($"Found window '{window.Title}' with handle = {window.Handle}.");
			}

			logger.Info("Minimizing all open windows...");
			nativeMethods.MinimizeAllOpenWindows();
			logger.Info("Open windows successfully minimized.");
		}

		public void RestoreAllWindows()
		{
			logger.Info("Restoring all minimized windows...");

			foreach (var window in minimizedWindows)
			{
				nativeMethods.RestoreWindow(window.Handle);
				logger.Info($"Restored window '{window.Title}' with handle = {window.Handle}.");
			}

			minimizedWindows.Clear();
			logger.Info("Minimized windows successfully restored.");
		}

		public void Start()
		{
			var process = new System.Diagnostics.Process();
			var explorerPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");

			logger.Debug("Starting explorer shell process...");

			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.FileName = explorerPath;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
			process.Start();

			logger.Debug("Waiting for explorer shell to initialize...");

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
			const int THREE_SECONDS = 3000;
			var processId = nativeMethods.GetShellProcessId();
			var explorerProcesses = System.Diagnostics.Process.GetProcessesByName("explorer");
			var process = explorerProcesses.FirstOrDefault(p => p.Id == processId);

			if (process != null)
			{
				logger.Debug($"Found explorer shell processes with PID = {processId}. Sending close message...");
				nativeMethods.PostCloseMessageToShell();
				logger.Debug("Waiting for explorer shell to terminate...");

				for (var elapsed = 0; nativeMethods.GetShellWindowHandle() != IntPtr.Zero && elapsed < THREE_SECONDS; elapsed += 20)
				{
					Thread.Sleep(20);
				}

				process.WaitForExit(THREE_SECONDS);
				process.Refresh();

				if (!process.HasExited)
				{
					KillExplorerShell(process.Id);
				}

				process.Refresh();

				if (process.HasExited)
				{
					logger.Info($"Successfully terminated explorer shell process with PID = {processId}.");
				}
				else
				{
					logger.Error($"Failed to completely terminate explorer shell process with PID = {processId}.");
				}

				process.Close();
			}
			else
			{
				logger.Info("The explorer shell seems to already be terminated.");
			}
		}

		private void KillExplorerShell(int processId)
		{
			var process = new System.Diagnostics.Process();
			var taskkillPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "taskkill.exe");

			logger.Warn("Failed to gracefully terminate explorer shell, attempting forceful termination...");

			process.StartInfo.Arguments = $"/F /PID {processId}";
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.FileName = taskkillPath;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			process.StartInfo.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
			process.Start();
			process.WaitForExit();
		}
	}
}
