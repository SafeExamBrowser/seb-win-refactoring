/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
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

namespace SafeExamBrowser.Client.Behaviour.Operations
{
	internal class WindowMonitorOperation : IOperation
	{
		private ILogger logger;
		private IWindowMonitor windowMonitor;

		public ISplashScreen SplashScreen { private get; set; }

		public WindowMonitorOperation(ILogger logger, IWindowMonitor windowMonitor)
		{
			this.logger = logger;
			this.windowMonitor = windowMonitor;
		}

		public void Perform()
		{
			logger.Info("Initializing window monitoring...");
			SplashScreen.UpdateText(TextKey.SplashScreen_InitializeWindowMonitoring);

			windowMonitor.HideAllWindows();
			windowMonitor.StartMonitoringWindows();
		}

		public void Revert()
		{
			logger.Info("Stopping window monitoring...");
			SplashScreen.UpdateText(TextKey.SplashScreen_StopWindowMonitoring);

			windowMonitor.StopMonitoringWindows();
			windowMonitor.RestoreHiddenWindows();
		}
	}
}
