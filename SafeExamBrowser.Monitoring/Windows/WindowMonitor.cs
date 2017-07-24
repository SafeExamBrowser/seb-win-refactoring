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
using SafeExamBrowser.WindowsApi;

namespace SafeExamBrowser.Monitoring.Windows
{
	public class WindowMonitor : IWindowMonitor
	{
		private ILogger logger;
		private IList<Window> minimizedWindows = new List<Window>();

		public WindowMonitor(ILogger logger)
		{
			this.logger = logger;
		}

		public void HideAllWindows()
		{
			logger.Info("Saving windows to be minimized...");

			foreach (var handle in User32.GetOpenWindows())
			{
				var window = new Window
				{
					Handle = handle,
					Title = User32.GetWindowTitle(handle)
				};

				minimizedWindows.Add(window);
				logger.Info($"Saved window '{window.Title}' with handle = {window.Handle}.");
			}

			logger.Info("Minimizing all open windows...");
			User32.MinimizeAllOpenWindows();
			logger.Info("Open windows successfully minimized.");
		}

		public void RestoreHiddenWindows()
		{
			logger.Info("Restoring all minimized windows...");
			
			foreach (var window in minimizedWindows)
			{
				User32.RestoreWindow(window.Handle);
				logger.Info($"Restored window '{window.Title}' with handle = {window.Handle}.");
			}

			logger.Info("Minimized windows successfully restored.");
		}

		public void StartMonitoringWindows()
		{
			// TODO
		}

		public void StopMonitoringWindows()
		{
			// TODO
		}

		private struct Window
		{
			internal IntPtr Handle { get; set; }
			internal string Title { get; set; }
		}
	}
}
