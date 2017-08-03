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
	public class ProcessMonitorOperation : IOperation
	{
		private ILogger logger;
		private IProcessMonitor processMonitor;

		public ISplashScreen SplashScreen { private get; set; }

		public ProcessMonitorOperation(ILogger logger, IProcessMonitor processMonitor)
		{
			this.logger = logger;
			this.processMonitor = processMonitor;
		}

		public void Perform()
		{
			logger.Info("Initializing process monitoring...");
			SplashScreen.UpdateText(TextKey.SplashScreen_WaitExplorerTermination, true);

			processMonitor.CloseExplorerShell();
			processMonitor.StartMonitoringExplorer();

			SplashScreen.UpdateText(TextKey.SplashScreen_InitializeProcessMonitoring);

			// TODO
		}

		public void Revert()
		{
			logger.Info("Stopping process monitoring...");
			SplashScreen.UpdateText(TextKey.SplashScreen_StopProcessMonitoring);

			// TODO

			SplashScreen.UpdateText(TextKey.SplashScreen_WaitExplorerStartup, true);

			processMonitor.StopMonitoringExplorer();
			processMonitor.StartExplorerShell();
		}
	}
}
