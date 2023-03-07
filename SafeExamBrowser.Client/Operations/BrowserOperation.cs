/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;

namespace SafeExamBrowser.Client.Operations
{
	internal class BrowserOperation : ClientOperation
	{
		private IActionCenter actionCenter;
		private ILogger logger;
		private ITaskbar taskbar;
		private ITaskview taskview;
		private IUserInterfaceFactory uiFactory;

		public override event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public override event StatusChangedEventHandler StatusChanged;

		public BrowserOperation(
			IActionCenter actionCenter,
			ClientContext context,
			ILogger logger,
			ITaskbar taskbar,
			ITaskview taskview,
			IUserInterfaceFactory uiFactory) : base(context)
		{
			this.actionCenter = actionCenter;
			this.logger = logger;
			this.taskbar = taskbar;
			this.taskview = taskview;
			this.uiFactory = uiFactory;
		}

		public override OperationResult Perform()
		{
			logger.Info("Initializing browser...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeBrowser);

			if (Context.Settings.Browser.EnableBrowser)
			{
				Context.Browser.Initialize();

				if (Context.Settings.ActionCenter.EnableActionCenter)
				{
					actionCenter.AddApplicationControl(uiFactory.CreateApplicationControl(Context.Browser, Location.ActionCenter), true);
				}

				if (Context.Settings.Taskbar.EnableTaskbar)
				{
					taskbar.AddApplicationControl(uiFactory.CreateApplicationControl(Context.Browser, Location.Taskbar), true);
				}

				taskview.Add(Context.Browser);
			}
			else
			{
				logger.Info("Browser application is disabled for this session.");
			}

			return OperationResult.Success;
		}

		public override OperationResult Revert()
		{
			logger.Info("Terminating browser...");
			StatusChanged?.Invoke(TextKey.OperationStatus_TerminateBrowser);

			if (Context.Settings.Browser.EnableBrowser)
			{
				Context.Browser.Terminate();
			}
			else
			{
				logger.Info("Browser application was disabled for this session.");
			}

			return OperationResult.Success;
		}
	}
}
