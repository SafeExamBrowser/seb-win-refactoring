/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Core.OperationModel.Events;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;

namespace SafeExamBrowser.Client.Operations
{
	internal class ProcessMonitorOperation : IOperation
	{
		private ILogger logger;
		private IProcessMonitor processMonitor;
		private Settings settings;

		public event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public event StatusChangedEventHandler StatusChanged;

		public ProcessMonitorOperation(ILogger logger, IProcessMonitor processMonitor, Settings settings)
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

		public OperationResult Repeat()
		{
			throw new InvalidOperationException($"The '{nameof(ProcessMonitorOperation)}' is not meant to be repeated!");
		}

		public void Revert()
		{
			logger.Info("Stopping process monitoring...");
			StatusChanged?.Invoke(TextKey.OperationStatus_StopProcessMonitoring);

			if (settings.KioskMode == KioskMode.DisableExplorerShell)
			{
				processMonitor.StopMonitoringExplorer();
			}
		}
	}
}
