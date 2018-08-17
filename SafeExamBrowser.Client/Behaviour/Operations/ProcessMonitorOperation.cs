/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Behaviour.OperationModel;
using SafeExamBrowser.Contracts.Configuration.Settings;
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
		private Settings settings;

		public IProgressIndicator ProgressIndicator { private get; set; }

		public ProcessMonitorOperation(ILogger logger, IProcessMonitor processMonitor, Settings settings)
		{
			this.logger = logger;
			this.processMonitor = processMonitor;
			this.settings = settings;
		}

		public OperationResult Perform()
		{
			logger.Info("Initializing process monitoring...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_InitializeProcessMonitoring);

			if (settings.KioskMode == KioskMode.DisableExplorerShell)
			{
				processMonitor.StartMonitoringExplorer();
			}

			return OperationResult.Success;
		}

		public OperationResult Repeat()
		{
			return OperationResult.Success;
		}

		public void Revert()
		{
			logger.Info("Stopping process monitoring...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_StopProcessMonitoring);

			if (settings.KioskMode == KioskMode.DisableExplorerShell)
			{
				processMonitor.StopMonitoringExplorer();
			}
		}
	}
}
