/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Applications.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;

namespace SafeExamBrowser.Client.Operations
{
	internal class BrowserOperation : IOperation
	{
		private IActionCenter actionCenter;
		private IApplication browser;
		private ILogger logger;
		private ITaskbar taskbar;
		private IUserInterfaceFactory uiFactory;

		public event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public event StatusChangedEventHandler StatusChanged;

		public BrowserOperation(
			IActionCenter actionCenter,
			IApplication browser,
			ILogger logger,
			ITaskbar taskbar,
			IUserInterfaceFactory uiFactory)
		{
			this.actionCenter = actionCenter;
			this.browser = browser;
			this.logger = logger;
			this.taskbar = taskbar;
			this.uiFactory = uiFactory;
		}

		public OperationResult Perform()
		{
			logger.Info("Initializing browser...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeBrowser);

			browser.Initialize();

			actionCenter.AddApplicationControl(uiFactory.CreateApplicationControl(browser, Location.ActionCenter));
			taskbar.AddApplicationControl(uiFactory.CreateApplicationControl(browser, Location.Taskbar));

			return OperationResult.Success;
		}

		public OperationResult Revert()
		{
			logger.Info("Terminating browser...");
			StatusChanged?.Invoke(TextKey.OperationStatus_TerminateBrowser);

			browser.Terminate();

			return OperationResult.Success;
		}
	}
}
