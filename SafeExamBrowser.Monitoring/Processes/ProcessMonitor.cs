/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics;
using System.Management;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.Contracts.Monitoring.Events;
using SafeExamBrowser.Contracts.WindowsApi;

namespace SafeExamBrowser.Monitoring.Processes
{
	public class ProcessMonitor : IProcessMonitor
	{
		private ILogger logger;
		private INativeMethods nativeMethods;
		private ManagementEventWatcher explorerWatcher;

		public event ExplorerStartedEventHandler ExplorerStarted;

		public ProcessMonitor(ILogger logger, INativeMethods nativeMethods)
		{
			this.logger = logger;
			this.nativeMethods = nativeMethods;
		}

		public bool BelongsToAllowedProcess(IntPtr window)
		{
			var processId = nativeMethods.GetProcessIdFor(window);
			var process = Process.GetProcessById(Convert.ToInt32(processId));

			if (process != null)
			{
				var allowed = process.ProcessName == "SafeExamBrowser";

				if (!allowed)
				{
					logger.Warn($"Window with handle = {window} belongs to not allowed process '{process.ProcessName}'!");
				}

				return allowed;
			}

			return true;
		}

		public void StartMonitoringExplorer()
		{
			explorerWatcher = new ManagementEventWatcher(@"\\.\root\CIMV2", GetQueryFor("explorer.exe"));
			explorerWatcher.EventArrived += new EventArrivedEventHandler(ExplorerWatcher_EventArrived);
			explorerWatcher.Start();

			logger.Info("Started monitoring process 'explorer.exe'.");
		}

		public void StopMonitoringExplorer()
		{
			explorerWatcher?.Stop();
			logger.Info("Stopped monitoring 'explorer.exe'.");
		}

		private void ExplorerWatcher_EventArrived(object sender, EventArrivedEventArgs e)
		{
			var eventName = e.NewEvent.ClassPath.ClassName;

			if (eventName == "__InstanceCreationEvent")
			{
				logger.Warn("A new instance of Windows explorer has been started!");
				ExplorerStarted?.Invoke();
			}
		}

		private string GetQueryFor(string processName)
		{
			return $@"
				SELECT *
				FROM __InstanceOperationEvent
				WITHIN 2
				WHERE TargetInstance ISA 'Win32_Process'
				AND TargetInstance.Name = '{processName}'";
		}
	}
}
