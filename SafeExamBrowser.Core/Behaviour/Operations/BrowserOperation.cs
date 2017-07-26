/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Core.Behaviour.Operations
{
	public class BrowserOperation : IOperation
	{
		private IApplicationController browserController;
		private IApplicationInfo browserInfo;
		private ILogger logger;
		private ITaskbar taskbar;
		private IUiElementFactory uiFactory;

		public ISplashScreen SplashScreen { private get; set; }

		public BrowserOperation(
			IApplicationController browserController,
			IApplicationInfo browserInfo,
			ILogger logger,
			ITaskbar taskbar,
			IUiElementFactory uiFactory)
		{
			this.browserController = browserController;
			this.browserInfo = browserInfo;
			this.logger = logger;
			this.taskbar = taskbar;
			this.uiFactory = uiFactory;
		}

		public void Perform()
		{
			logger.Info("Initializing browser...");
			SplashScreen.UpdateText(Key.SplashScreen_InitializeBrowser);

			var browserButton = uiFactory.CreateApplicationButton(browserInfo);

			browserController.Initialize();
			browserController.RegisterApplicationButton(browserButton);

			taskbar.AddButton(browserButton);
		}

		public void Revert()
		{
			logger.Info("Terminating browser...");
			SplashScreen.UpdateText(Key.SplashScreen_TerminateBrowser);

			browserController.Terminate();
		}
	}
}
