/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Core.OperationModel.Events;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;

namespace SafeExamBrowser.Client.Operations
{
	internal class DisplayMonitorOperation : IOperation
	{
		private IDisplayMonitor displayMonitor;
		private ILogger logger;
		private ITaskbar taskbar;

		public event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public event StatusChangedEventHandler StatusChanged;

		public DisplayMonitorOperation(IDisplayMonitor displayMonitor, ILogger logger, ITaskbar taskbar)
		{
			this.displayMonitor = displayMonitor;
			this.logger = logger;
			this.taskbar = taskbar;
		}

		public OperationResult Perform()
		{
			logger.Info("Initializing working area...");
			StatusChanged?.Invoke(TextKey.ProgressIndicator_InitializeWorkingArea);

			displayMonitor.PreventSleepMode();
			displayMonitor.InitializePrimaryDisplay(taskbar.GetAbsoluteHeight());
			displayMonitor.StartMonitoringDisplayChanges();

			return OperationResult.Success;
		}

		public OperationResult Repeat()
		{
			return OperationResult.Success;
		}

		public void Revert()
		{
			logger.Info("Restoring working area...");
			StatusChanged?.Invoke(TextKey.ProgressIndicator_RestoreWorkingArea);

			displayMonitor.StopMonitoringDisplayChanges();
			displayMonitor.ResetPrimaryDisplay();
		}
	}
}
