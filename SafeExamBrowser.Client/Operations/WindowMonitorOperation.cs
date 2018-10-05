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

			if (kioskMode == KioskMode.DisableExplorerShell)
			{
				windowMonitor.HideAllWindows();
			}

			if (kioskMode != KioskMode.None)
			{
				windowMonitor.StartMonitoringWindows();
			}

			return OperationResult.Success;
		}

		public OperationResult Repeat()
		{
			throw new InvalidOperationException($"The '{nameof(WindowMonitorOperation)}' is not meant to be repeated!");
		}

		public void Revert()
		{
			logger.Info("Stopping window monitoring...");
			StatusChanged?.Invoke(TextKey.OperationStatus_StopWindowMonitoring);

			if (kioskMode != KioskMode.None)
			{
				windowMonitor.StopMonitoringWindows();
			}

			if (kioskMode == KioskMode.DisableExplorerShell)
			{
				windowMonitor.RestoreHiddenWindows();
			}
		}
	}
}
