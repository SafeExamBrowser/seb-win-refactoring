/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Behaviour.Operations;
using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;

namespace SafeExamBrowser.Client.Behaviour
{
	internal class ClientController : IClientController
	{
		private IDisplayMonitor displayMonitor;
		private ILogger logger;
		private IOperationSequence operations;
		private IProcessMonitor processMonitor;
		private IRuntimeProxy runtime;
		private ITaskbar taskbar;
		private IWindowMonitor windowMonitor;

		public ClientController(
			IDisplayMonitor displayMonitor,
			ILogger logger,
			IOperationSequence operations,
			IProcessMonitor processMonitor,
			IRuntimeProxy runtime,
			ITaskbar taskbar,
			IWindowMonitor windowMonitor)
		{
			this.displayMonitor = displayMonitor;
			this.logger = logger;
			this.operations = operations;
			this.processMonitor = processMonitor;
			this.runtime = runtime;
			this.taskbar = taskbar;
			this.windowMonitor = windowMonitor;
		}

		public void Terminate()
		{
			displayMonitor.DisplayChanged -= DisplayMonitor_DisplaySettingsChanged;
			processMonitor.ExplorerStarted -= ProcessMonitor_ExplorerStarted;
			windowMonitor.WindowChanged -= WindowMonitor_WindowChanged;

			// TODO

			operations.TryRevert();
		}

		public bool TryStart()
		{

			// TODO

			var success = operations.TryPerform();

			if (success)
			{
				displayMonitor.DisplayChanged += DisplayMonitor_DisplaySettingsChanged;
				processMonitor.ExplorerStarted += ProcessMonitor_ExplorerStarted;
				windowMonitor.WindowChanged += WindowMonitor_WindowChanged;

				runtime.InformClientReady();
			}

			return success;
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
