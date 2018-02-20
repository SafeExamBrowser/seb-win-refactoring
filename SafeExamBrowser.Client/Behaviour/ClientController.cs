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

		public IClientHost ClientHost { private get; set; }

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

		public bool TryStart()
		{
			var success = operations.TryPerform();

			// TODO

			if (success)
			{
				RegisterEvents();
				runtime.InformClientReady();
			}

			return success;
		}

		public void Terminate()
		{
			DeregisterEvents();

			// TODO

			operations.TryRevert();
		}

		private void RegisterEvents()
		{
			ClientHost.Shutdown += ClientHost_Shutdown;
			displayMonitor.DisplayChanged += DisplayMonitor_DisplaySettingsChanged;
			processMonitor.ExplorerStarted += ProcessMonitor_ExplorerStarted;
			taskbar.QuitButtonClicked += Taskbar_QuitButtonClicked;
			windowMonitor.WindowChanged += WindowMonitor_WindowChanged;
		}

		private void DeregisterEvents()
		{
			ClientHost.Shutdown -= ClientHost_Shutdown;
			displayMonitor.DisplayChanged -= DisplayMonitor_DisplaySettingsChanged;
			processMonitor.ExplorerStarted -= ProcessMonitor_ExplorerStarted;
			taskbar.QuitButtonClicked -= Taskbar_QuitButtonClicked;
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

		private void ClientHost_Shutdown()
		{
			// TODO: Better use callback to Application.Shutdown() as in runtime?
			taskbar.Close();
		}

		private void Taskbar_QuitButtonClicked()
		{
			// TODO: MessageBox asking whether user really wants to quit -> args.Cancel

			var acknowledged = runtime.RequestShutdown();

			if (!acknowledged)
			{
				logger.Warn("The runtime did not acknowledge the shutdown request!");
			}
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
