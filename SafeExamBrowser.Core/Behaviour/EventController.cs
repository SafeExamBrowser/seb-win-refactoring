/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Core.Behaviour
{
	public class EventController : IEventController
	{
		private ILogger logger;
		private IProcessMonitor processMonitor;
		private ITaskbar taskbar;
		private IWindowMonitor windowMonitor;
		private IWorkingArea workingArea;

		public EventController(
			ILogger logger,
			IProcessMonitor processMonitor,
			ITaskbar taskbar,
			IWindowMonitor windowMonitor,
			IWorkingArea workingArea)
		{
			this.logger = logger;
			this.processMonitor = processMonitor;
			this.taskbar = taskbar;
			this.windowMonitor = windowMonitor;
			this.workingArea = workingArea;
		}

		public void Start()
		{
			processMonitor.ExplorerStarted += ProcessMonitor_ExplorerStarted;
			windowMonitor.WindowChanged += processMonitor.OnWindowChanged;
		}

		public void Stop()
		{
			processMonitor.ExplorerStarted -= ProcessMonitor_ExplorerStarted;
			windowMonitor.WindowChanged -= processMonitor.OnWindowChanged;
		}

		private void ProcessMonitor_ExplorerStarted()
		{
			logger.Info("Trying to shut down explorer...");
			processMonitor.CloseExplorerShell();
			logger.Info("Reinitializing working area...");
			workingArea.InitializeFor(taskbar);
			logger.Info("Reinitializing taskbar bounds...");
			taskbar.InitializeBounds();
			logger.Info("Desktop successfully restored!");
		}
	}
}
