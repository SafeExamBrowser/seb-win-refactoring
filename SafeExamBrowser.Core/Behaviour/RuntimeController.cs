/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Core.Behaviour
{
	public class RuntimeController : IRuntimeController
	{
		private IDisplayMonitor displayMonitor;
		private ILogger logger;
		private IProcessMonitor processMonitor;
		private ITaskbar taskbar;
		private IWindowMonitor windowMonitor;

		public RuntimeController(
			IDisplayMonitor displayMonitor,
			ILogger logger,
			IProcessMonitor processMonitor,
			ITaskbar taskbar,
			IWindowMonitor windowMonitor)
		{
			this.displayMonitor = displayMonitor;
			this.logger = logger;
			this.processMonitor = processMonitor;
			this.taskbar = taskbar;
			this.windowMonitor = windowMonitor;
		}

		public void Start()
		{
			displayMonitor.DisplayChanged += DisplayMonitor_DisplaySettingsChanged;
			processMonitor.ExplorerStarted += ProcessMonitor_ExplorerStarted;
			windowMonitor.WindowChanged += WindowMonitor_WindowChanged;
		}

		public void Stop()
		{
			processMonitor.ExplorerStarted -= ProcessMonitor_ExplorerStarted;
			windowMonitor.WindowChanged -= WindowMonitor_WindowChanged;
		}

		private void DisplayMonitor_DisplaySettingsChanged()
		{
			logger.Info("Reinitializing working area...");
			displayMonitor.InitializePrimaryDisplay(taskbar.GetAbsoluteHeight());
			logger.Info("Reinitializing taskbar bounds...");
			taskbar.InitializeBounds();
			logger.Info("Desktop successfully restored.");
		}

		private void ProcessMonitor_ExplorerStarted()
		{
			logger.Info("Trying to shut down explorer...");
			processMonitor.CloseExplorerShell();
			logger.Info("Reinitializing working area...");
			displayMonitor.InitializePrimaryDisplay(taskbar.GetAbsoluteHeight());
			logger.Info("Reinitializing taskbar bounds...");
			taskbar.InitializeBounds();
			logger.Info("Desktop successfully restored.");
		}

		private void WindowMonitor_WindowChanged(IntPtr window)
		{
			var allowed = processMonitor.BelongsToAllowedProcess(window);

			if (!allowed)
			{
				var success = windowMonitor.Hide(window);

				if (!success)
				{
					windowMonitor.Close(window);
				}
			}
		}
	}
}
