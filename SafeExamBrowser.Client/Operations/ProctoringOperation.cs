/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Proctoring.Contracts;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Client.Operations
{
	internal class ProctoringOperation : ClientOperation
	{
		private readonly IActionCenter actionCenter;
		private readonly IProctoringController controller;
		private readonly ILogger logger;
		private readonly IMessageBox messageBox;
		private readonly ISplashScreen splashScreen;
		private readonly ITaskbar taskbar;
		private readonly IUserInterfaceFactory uiFactory;

		public override event StatusChangedEventHandler StatusChanged;

		public ProctoringOperation(
			IActionCenter actionCenter,
			ClientContext context,
			IProctoringController controller,
			ILogger logger,
			IMessageBox messageBox,
			ISplashScreen splashScreen,
			ITaskbar taskbar,
			IUserInterfaceFactory uiFactory) : base(context)
		{
			this.actionCenter = actionCenter;
			this.controller = controller;
			this.logger = logger;
			this.messageBox = messageBox;
			this.splashScreen = splashScreen;
			this.taskbar = taskbar;
			this.uiFactory = uiFactory;
		}

		public override OperationResult Perform()
		{
			var result = OperationResult.Success;

			if (Context.Settings.Proctoring.Enabled)
			{
				logger.Info("Initializing proctoring...");
				StatusChanged?.Invoke(TextKey.OperationStatus_InitializeProctoring);

				var success = controller.Initialize(Context.Settings.Proctoring);
				result = success ? OperationResult.Success : OperationResult.Failed;

				if (success)
				{
					AddNotificationControls();
				}
				else
				{
					InformAboutInitializationFailure();
				}
			}

			return result;
		}

		public override OperationResult Revert()
		{
			if (Context.Settings.Proctoring.Enabled)
			{
				logger.Info("Terminating proctoring...");
				StatusChanged?.Invoke(TextKey.OperationStatus_TerminateProctoring);

				controller.Terminate();

				foreach (var notification in controller.Notifications)
				{
					notification.Terminate();
				}
			}

			return OperationResult.Success;
		}

		private void AddNotificationControls()
		{
			foreach (var notification in controller.Notifications)
			{
				actionCenter.AddNotificationControl(uiFactory.CreateNotificationControl(notification, Location.ActionCenter));

				if (Context.Settings.Proctoring.ShowTaskbarNotification)
				{
					taskbar.AddNotificationControl(uiFactory.CreateNotificationControl(notification, Location.Taskbar));
				}
			}
		}

		private void InformAboutInitializationFailure()
		{
			var message = TextKey.MessageBox_ProctoringInitializationFailure;
			var title = TextKey.MessageBox_ProctoringInitializationFailureTitle;

			messageBox.Show(message, title, icon: MessageBoxIcon.Error, parent: splashScreen);
		}
	}
}
