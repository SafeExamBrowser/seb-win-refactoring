/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts.Display;
using SafeExamBrowser.UserInterface.Contracts.Shell;

namespace SafeExamBrowser.Client.Operations
{
	internal class DisplayMonitorOperation : ClientOperation
	{
		private readonly IDisplayMonitor displayMonitor;
		private readonly ILogger logger;
		private readonly ITaskbar taskbar;

		public override event StatusChangedEventHandler StatusChanged;

		public DisplayMonitorOperation(ClientContext context, IDisplayMonitor displayMonitor, ILogger logger, ITaskbar taskbar) : base(context)
		{
			this.displayMonitor = displayMonitor;
			this.logger = logger;
			this.taskbar = taskbar;
		}

		public override OperationResult Perform()
		{
			logger.Info("Initializing working area...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeWorkingArea);

			displayMonitor.InitializePrimaryDisplay(Context.Settings.UserInterface.Taskbar.EnableTaskbar ? taskbar.GetAbsoluteHeight() : 0);
			displayMonitor.StartMonitoringDisplayChanges();

			return OperationResult.Success;
		}

		public override OperationResult Revert()
		{
			logger.Info("Restoring working area...");
			StatusChanged?.Invoke(TextKey.OperationStatus_RestoreWorkingArea);

			displayMonitor.StopMonitoringDisplayChanges();
			displayMonitor.ResetPrimaryDisplay();

			return OperationResult.Success;
		}
	}
}
