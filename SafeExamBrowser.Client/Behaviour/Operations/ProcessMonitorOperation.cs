/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Behaviour.Operations;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Client.Behaviour.Operations
{
	internal class ProcessMonitorOperation : IOperation
	{
		private ILogger logger;
		private IProcessMonitor processMonitor;

		public bool Abort { get; private set; }
		public IProgressIndicator ProgressIndicator { private get; set; }

		public ProcessMonitorOperation(ILogger logger, IProcessMonitor processMonitor)
		{
			this.logger = logger;
			this.processMonitor = processMonitor;
		}

		public void Perform()
		{
			logger.Info("Initializing process monitoring...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_WaitExplorerTermination, true);

			processMonitor.CloseExplorerShell();
			processMonitor.StartMonitoringExplorer();

			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_InitializeProcessMonitoring);

			// TODO: Implement process monitoring...
		}

		public void Repeat()
		{
			// Nothing to do here...
		}

		public void Revert()
		{
			logger.Info("Stopping process monitoring...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_StopProcessMonitoring);

			// TODO: Implement process monitoring...

			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_WaitExplorerStartup, true);

			processMonitor.StopMonitoringExplorer();
			processMonitor.StartExplorerShell();
		}
	}
}
