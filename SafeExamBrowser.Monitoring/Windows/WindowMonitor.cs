/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Events;
using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Monitoring.Windows
{
	public class WindowMonitor : IWindowMonitor
	{
		private IntPtr activeWindow;
		private Guid? captureHookId;
		private Guid? foregroundHookId;
		private ILogger logger;
		private INativeMethods nativeMethods;

		public event WindowChangedEventHandler WindowChanged;

		public WindowMonitor(ILogger logger, INativeMethods nativeMethods)
		{
			this.logger = logger;
			this.nativeMethods = nativeMethods;
		}

		public void Close(IntPtr window)
		{
			var title = nativeMethods.GetWindowTitle(window);

			nativeMethods.SendCloseMessageTo(window);
			logger.Info($"Sent close message to window '{title}' with handle = {window}.");
		}

		public bool Hide(IntPtr window)
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

		public void StartMonitoringWindows()
		{
			captureHookId = nativeMethods.RegisterSystemCaptureStartEvent(OnWindowChanged);
			logger.Info($"Registered system capture start event with ID = {captureHookId}.");

			foregroundHookId = nativeMethods.RegisterSystemForegroundEvent(OnWindowChanged);
			logger.Info($"Registered system foreground event with ID = {foregroundHookId}.");
		}

		public void StopMonitoringWindows()
		{
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

		private void OnWindowChanged(IntPtr window)
		{
			if (window != IntPtr.Zero && activeWindow != window)
			{
				logger.Debug($"Window has changed from {activeWindow} to {window}.");
				activeWindow = window;
				WindowChanged?.Invoke(window);
			}
		}
	}
}
