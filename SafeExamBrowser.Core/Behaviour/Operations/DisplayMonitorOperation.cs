/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Core.Behaviour.Operations
{
	public class DisplayMonitorOperation : IOperation
	{
		private IDisplayMonitor displayMonitor;
		private ILogger logger;
		private ITaskbar taskbar;

		public ISplashScreen SplashScreen { private get; set; }

		public DisplayMonitorOperation(IDisplayMonitor displayMonitor, ILogger logger, ITaskbar taskbar)
		{
			this.displayMonitor = displayMonitor;
			this.logger = logger;
			this.taskbar = taskbar;
		}

		public void Perform()
		{
			logger.Info("Initializing working area...");
			SplashScreen.UpdateText(TextKey.SplashScreen_InitializeWorkingArea);

			displayMonitor.PreventSleepMode();
			displayMonitor.InitializePrimaryDisplay(taskbar.GetAbsoluteHeight());
			displayMonitor.StartMonitoringDisplayChanges();
		}

		public void Revert()
		{
			logger.Info("Restoring working area...");
			SplashScreen.UpdateText(TextKey.SplashScreen_RestoreWorkingArea);

			displayMonitor.StopMonitoringDisplayChanges();
			displayMonitor.ResetPrimaryDisplay();
		}
	}
}
