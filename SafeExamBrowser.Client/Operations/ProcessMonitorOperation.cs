/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Settings;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Processes;

namespace SafeExamBrowser.Client.Operations
{
	internal class ProcessMonitorOperation : IOperation
	{
		private ILogger logger;
		private IProcessMonitor processMonitor;
		private ApplicationSettings settings;

		public event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public event StatusChangedEventHandler StatusChanged;

		public ProcessMonitorOperation(ILogger logger, IProcessMonitor processMonitor, ApplicationSettings settings)
		{
			this.logger = logger;
			this.processMonitor = processMonitor;
			this.settings = settings;
		}

		public OperationResult Perform()
		{
			logger.Info("Initializing process monitoring...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeProcessMonitoring);

			if (settings.KioskMode == KioskMode.DisableExplorerShell)
			{
				processMonitor.StartMonitoringExplorer();
			}

			return OperationResult.Success;
		}

		public OperationResult Revert()
		{
			logger.Info("Stopping process monitoring...");
			StatusChanged?.Invoke(TextKey.OperationStatus_StopProcessMonitoring);

			if (settings.KioskMode == KioskMode.DisableExplorerShell)
			{
				processMonitor.StopMonitoringExplorer();
			}

			return OperationResult.Success;
		}
	}
}
