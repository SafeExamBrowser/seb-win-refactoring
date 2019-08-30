/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Configuration.Contracts.Settings;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring.Contracts;

namespace SafeExamBrowser.Client.Operations
{
	internal class WindowMonitorOperation : IOperation
	{
		private KioskMode kioskMode;
		private ILogger logger;
		private IWindowMonitor windowMonitor;

		public event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public event StatusChangedEventHandler StatusChanged;

		public WindowMonitorOperation(KioskMode kioskMode, ILogger logger, IWindowMonitor windowMonitor)
		{
			this.kioskMode = kioskMode;
			this.logger = logger;
			this.windowMonitor = windowMonitor;
		}

		public OperationResult Perform()
		{
			logger.Info("Initializing window monitoring...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeWindowMonitoring);

			if (kioskMode != KioskMode.None)
			{
				windowMonitor.StartMonitoringWindows();
			}

			return OperationResult.Success;
		}

		public OperationResult Revert()
		{
			logger.Info("Stopping window monitoring...");
			StatusChanged?.Invoke(TextKey.OperationStatus_StopWindowMonitoring);

			if (kioskMode != KioskMode.None)
			{
				windowMonitor.StopMonitoringWindows();
			}

			return OperationResult.Success;
		}
	}
}
