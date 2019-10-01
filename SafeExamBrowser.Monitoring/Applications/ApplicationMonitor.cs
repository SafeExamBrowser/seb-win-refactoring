/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics;
using System.Management;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Applications;
using SafeExamBrowser.Monitoring.Contracts.Applications.Events;
using SafeExamBrowser.Settings.Applications;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Monitoring.Applications
{
	public class ApplicationMonitor : IApplicationMonitor
	{
		private IntPtr activeWindow;
		private Guid? captureHookId;
		private Guid? foregroundHookId;
		private ILogger logger;
		private INativeMethods nativeMethods;
		private ManagementEventWatcher explorerWatcher;

		public event ExplorerStartedEventHandler ExplorerStarted;

		public ApplicationMonitor(ILogger logger, INativeMethods nativeMethods)
		{
			this.logger = logger;
			this.nativeMethods = nativeMethods;
		}

		public InitializationResult Initialize(ApplicationSettings settings)
		{
			// TODO
			// Initialize blacklist
			// Initialize whitelist
			// Check for running processes

			return new InitializationResult();
		}

		public void Start()
		{
			// TODO: Start monitoring blacklist...

			explorerWatcher = new ManagementEventWatcher(@"\\.\root\CIMV2", GetQueryFor("explorer.exe"));
			explorerWatcher.EventArrived += new EventArrivedEventHandler(ExplorerWatcher_EventArrived);
			explorerWatcher.Start();
			logger.Info("Started monitoring process 'explorer.exe'.");

			captureHookId = nativeMethods.RegisterSystemCaptureStartEvent(SystemEvent_WindowChanged);
			logger.Info($"Registered system capture start event with ID = {captureHookId}.");

			foregroundHookId = nativeMethods.RegisterSystemForegroundEvent(SystemEvent_WindowChanged);
			logger.Info($"Registered system foreground event with ID = {foregroundHookId}.");
		}

		public void Stop()
		{
			explorerWatcher?.Stop();
			logger.Info("Stopped monitoring 'explorer.exe'.");

			if (captureHookId.HasValue)
			{
				nativeMethods.DeregisterSystemEventHook(captureHookId.Value);
				logger.Info($"Unregistered system capture start event with ID = {captureHookId}.");
			}

			if (foregroundHookId.HasValue)
			{
				nativeMethods.DeregisterSystemEventHook(foregroundHookId.Value);
				logger.Info($"Unregistered system foreground event with ID = {foregroundHookId}.");
			}
		}

		public bool Terminate(int processId)
		{
			return false;
		}

		private void Check(IntPtr window)
		{
			var allowed = IsAllowed(window);

			if (!allowed)
			{
				var success = TryHide(window);

				if (!success)
				{
					Close(window);
				}
			}
		}

		private void Close(IntPtr window)
		{
			var title = nativeMethods.GetWindowTitle(window);

			nativeMethods.SendCloseMessageTo(window);
			logger.Info($"Sent close message to window '{title}' with handle = {window}.");
		}

		private bool IsAllowed(IntPtr window)
		{
			var processId = nativeMethods.GetProcessIdFor(window);
			var process = Process.GetProcessById(Convert.ToInt32(processId));

			if (process != null)
			{
				var allowed = process.ProcessName == "SafeExamBrowser" || process.ProcessName == "SafeExamBrowser.Client";

				if (!allowed)
				{
					logger.Warn($"Window with handle = {window} belongs to not allowed process '{process.ProcessName}'!");
				}

				return allowed;
			}

			return true;
		}

		private bool TryHide(IntPtr window)
		{
			var title = nativeMethods.GetWindowTitle(window);
			var success = nativeMethods.HideWindow(window);

			if (success)
			{
				logger.Info($"Hid window '{title}' with handle = {window}.");
			}
			else
			{
				logger.Warn($"Failed to hide window '{title}' with handle = {window}!");
			}

			return success;
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

		private void SystemEvent_WindowChanged(IntPtr window)
		{
			if (window != IntPtr.Zero && activeWindow != window)
			{
				logger.Debug($"Window has changed from {activeWindow} to {window}.");
				activeWindow = window;
				Check(window);
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
