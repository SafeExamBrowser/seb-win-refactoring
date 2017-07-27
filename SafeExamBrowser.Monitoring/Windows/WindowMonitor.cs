/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.Contracts.WindowsApi;

namespace SafeExamBrowser.Monitoring.Windows
{
	public class WindowMonitor : IWindowMonitor
	{
		private IntPtr captureStartHookHandle;
		private IntPtr foregroundHookHandle;
		private ILogger logger;
		private IList<Window> minimizedWindows = new List<Window>();
		private INativeMethods nativeMethods;

		public event WindowChangedHandler WindowChanged;

		public WindowMonitor(ILogger logger, INativeMethods nativeMethods)
		{
			this.logger = logger;
			this.nativeMethods = nativeMethods;
		}

		public void HideAllWindows()
		{
			logger.Info("Saving windows to be minimized...");

			foreach (var handle in nativeMethods.GetOpenWindows())
			{
				var window = new Window
				{
					Handle = handle,
					Title = nativeMethods.GetWindowTitle(handle)
				};

				minimizedWindows.Add(window);
				logger.Info($"Saved window '{window.Title}' with handle = {window.Handle}.");
			}

			logger.Info("Minimizing all open windows...");
			nativeMethods.MinimizeAllOpenWindows();
			logger.Info("Open windows successfully minimized.");
		}

		public void RestoreHiddenWindows()
		{
			logger.Info("Restoring all minimized windows...");
			
			foreach (var window in minimizedWindows)
			{
				nativeMethods.RestoreWindow(window.Handle);
				logger.Info($"Restored window '{window.Title}' with handle = {window.Handle}.");
			}

			logger.Info("Minimized windows successfully restored.");
		}

		public void StartMonitoringWindows()
		{
			captureStartHookHandle = nativeMethods.RegisterSystemCaptureStartEvent(OnWindowChanged);
			logger.Info($"Registered system capture start event with handle = {captureStartHookHandle}.");

			foregroundHookHandle = nativeMethods.RegisterSystemForegroundEvent(OnWindowChanged);
			logger.Info($"Registered system foreground event with handle = {foregroundHookHandle}.");
		}

		public void StopMonitoringWindows()
		{
			if (captureStartHookHandle != IntPtr.Zero)
			{
				nativeMethods.UnregisterSystemEvent(captureStartHookHandle);
				logger.Info($"Unregistered system capture start event with handle = {captureStartHookHandle}.");
			}

			if (foregroundHookHandle != IntPtr.Zero)
			{
				nativeMethods.UnregisterSystemEvent(foregroundHookHandle);
				logger.Info($"Unregistered system foreground event with handle = {foregroundHookHandle}.");
			}
		}

		private void OnWindowChanged(IntPtr window)
		{
			if (WindowChanged != null)
			{
				WindowChanged.Invoke(window, out bool hide);

				if (hide)
				{
					var title = nativeMethods.GetWindowTitle(window);

					nativeMethods.HideWindow(window);
					logger.Info($"Hid window '{title}' with handle = {window}.");
				}
			}
		}

		private struct Window
		{
			internal IntPtr Handle { get; set; }
			internal string Title { get; set; }
		}
	}
}
