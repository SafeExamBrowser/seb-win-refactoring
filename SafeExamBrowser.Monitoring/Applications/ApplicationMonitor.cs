/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading;
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
		private IList<BlacklistApplication> blacklist;
		private Guid? captureHookId;
		private ManagementEventWatcher explorerWatcher;
		private Guid? foregroundHookId;
		private ILogger logger;
		private INativeMethods nativeMethods;
		private IProcessFactory processFactory;
		private IList<WhitelistApplication> whitelist;

		public event ExplorerStartedEventHandler ExplorerStarted;

		public ApplicationMonitor(ILogger logger, INativeMethods nativeMethods, IProcessFactory processFactory)
		{
			this.blacklist = new List<BlacklistApplication>();
			this.logger = logger;
			this.nativeMethods = nativeMethods;
			this.processFactory = processFactory;
			this.whitelist = new List<WhitelistApplication>();
		}

		public InitializationResult Initialize(ApplicationSettings settings)
		{
			var result = new InitializationResult();

			foreach (var application in settings.Blacklist)
			{
				blacklist.Add(application);
			}

			foreach (var application in settings.Whitelist)
			{
				whitelist.Add(application);
			}

			logger.Debug($"Initialized blacklist with {blacklist.Count} applications: {string.Join(", ", blacklist.Select(a => a.ExecutableName))}");
			logger.Debug($"Initialized whitelist with {whitelist.Count} applications: {string.Join(", ", whitelist.Select(a => a.ExecutableName))}");

			foreach (var process in processFactory.GetAllRunning())
			{
				foreach (var application in blacklist)
				{
					var isMatch = BelongsToApplication(process, application);

					if (isMatch && !application.AutoTerminate)
					{
						AddForTermination(application.ExecutableName, process, result);
					}
					else if (isMatch && application.AutoTerminate && !TryTerminate(process))
					{
						AddFailed(application.ExecutableName, process, result);
					}
				}

				foreach (var application in whitelist)
				{
					// TODO: Check if application is running, auto-terminate or add to result.
				}
			}

			return result;
		}

		public void Start()
		{
			// TODO: Start monitoring blacklist...

			// TODO: Remove WMI event and use timer mechanism!
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

		public bool TryTerminate(RunningApplication application)
		{
			var success = true;

			foreach (var process in application.Processes)
			{
				success &= TryTerminate(process);
			}

			return success;
		}

		private void AddFailed(string name, IProcess process, InitializationResult result)
		{
			var application = result.FailedAutoTerminations.FirstOrDefault(a => a.Name == name);

			if (application == default(RunningApplication))
			{
				application = new RunningApplication(name);
				result.FailedAutoTerminations.Add(application);
			}

			application.Processes.Add(process);
			logger.Error($"Process '{process.Name}' belongs to application '{application.Name}' and could not be terminated automatically!");
		}

		private void AddForTermination(string name, IProcess process, InitializationResult result)
		{
			var application = result.RunningApplications.FirstOrDefault(a => a.Name == name);

			if (application == default(RunningApplication))
			{
				application = new RunningApplication(name);
				result.RunningApplications.Add(application);
			}

			application.Processes.Add(process);
			logger.Debug($"Process '{process.Name}' belongs to application '{application.Name}' and needs to be terminated.");
		}

		private bool BelongsToApplication(IProcess process, BlacklistApplication application)
		{
			var sameName = process.Name.Equals(application.ExecutableName, StringComparison.OrdinalIgnoreCase);
			var sameOriginalName = process.OriginalName?.Equals(application.ExecutableOriginalName, StringComparison.OrdinalIgnoreCase) == true;

			return sameName || sameOriginalName;
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
			// TODO: Allow only if in whitelist!
			//var process = processFactory.GetById(Convert.ToInt32(processId));

			//if (process != null)
			//{
			//	var allowed = process.Name == "SafeExamBrowser" || process.Name == "SafeExamBrowser.Client";

			//	if (!allowed)
			//	{
			//		logger.Warn($"Window with handle = {window} belongs to not allowed process '{process.Name}'!");
			//	}

			//	return allowed;
			//}

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

		private bool TryTerminate(IProcess process)
		{
			const int MAX_ATTEMPTS = 5;
			const int TIMEOUT = 100;

			try
			{
				for (var attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
				{
					if (process.TryClose())
					{
						break;
					}
					else
					{
						Thread.Sleep(TIMEOUT);
					}
				}

				if (!process.HasTerminated)
				{
					for (var attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
					{
						if (process.TryKill())
						{
							break;
						}
						else
						{
							Thread.Sleep(TIMEOUT);
						}
					}
				}

				if (process.HasTerminated)
				{
					logger.Info($"Successfully terminated process '{process.Name}'.");
				}
				else
				{
					logger.Warn($"Failed to terminate process '{process.Name}'!");
				}
			}
			catch (Exception e)
			{
				logger.Error($"An error occurred while attempting to terminate process '{process.Name}'!", e);
			}

			return process.HasTerminated;
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
