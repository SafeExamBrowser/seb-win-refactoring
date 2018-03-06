/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Behaviour.OperationModel;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Client.Behaviour.Operations
{
	internal class WindowMonitorOperation : IOperation
	{
		private ILogger logger;
		private IWindowMonitor windowMonitor;

		public IProgressIndicator ProgressIndicator { private get; set; }

		public WindowMonitorOperation(ILogger logger, IWindowMonitor windowMonitor)
		{
			this.logger = logger;
			this.windowMonitor = windowMonitor;
		}

		public OperationResult Perform()
		{
			logger.Info("Initializing window monitoring...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_InitializeWindowMonitoring);

			windowMonitor.HideAllWindows();
			windowMonitor.StartMonitoringWindows();

			return OperationResult.Success;
		}

		public OperationResult Repeat()
		{
			return OperationResult.Success;
		}

		public void Revert()
		{
			logger.Info("Stopping window monitoring...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_StopWindowMonitoring);

			windowMonitor.StopMonitoringWindows();
			windowMonitor.RestoreHiddenWindows();
		}
	}
}
