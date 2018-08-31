/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Core;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;

namespace SafeExamBrowser.Client.Operations
{
	internal class BrowserOperation : IOperation
	{
		private IApplicationController browserController;
		private IApplicationInfo browserInfo;
		private ILogger logger;
		private ITaskbar taskbar;
		private IUserInterfaceFactory uiFactory;

		public IProgressIndicator ProgressIndicator { private get; set; }

		public BrowserOperation(
			IApplicationController browserController,
			IApplicationInfo browserInfo,
			ILogger logger,
			ITaskbar taskbar,
			IUserInterfaceFactory uiFactory)
		{
			this.browserController = browserController;
			this.browserInfo = browserInfo;
			this.logger = logger;
			this.taskbar = taskbar;
			this.uiFactory = uiFactory;
		}

		public OperationResult Perform()
		{
			logger.Info("Initializing browser...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_InitializeBrowser, true);

			var browserButton = uiFactory.CreateApplicationButton(browserInfo);

			browserController.Initialize();
			browserController.RegisterApplicationButton(browserButton);

			taskbar.AddApplication(browserButton);

			return OperationResult.Success;
		}

		public OperationResult Repeat()
		{
			return OperationResult.Success;
		}

		public void Revert()
		{
			logger.Info("Terminating browser...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_TerminateBrowser, true);

			browserController.Terminate();
		}
	}
}
