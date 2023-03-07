/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts.Notifications;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Proctoring.Contracts;
using SafeExamBrowser.Settings;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;

namespace SafeExamBrowser.Client.Operations
{
	internal class ProctoringOperation : ClientOperation
	{
		private readonly IActionCenter actionCenter;
		private readonly IProctoringController controller;
		private readonly ILogger logger;
		private readonly INotification notification;
		private readonly ITaskbar taskbar;
		private readonly IUserInterfaceFactory uiFactory;

		public override event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public override event StatusChangedEventHandler StatusChanged;

		public ProctoringOperation(
			IActionCenter actionCenter,
			ClientContext context,
			IProctoringController controller,
			ILogger logger,
			INotification notification,
			ITaskbar taskbar,
			IUserInterfaceFactory uiFactory) : base(context)
		{
			this.actionCenter = actionCenter;
			this.controller = controller;
			this.logger = logger;
			this.notification = notification;
			this.taskbar = taskbar;
			this.uiFactory = uiFactory;
		}

		public override OperationResult Perform()
		{
			if (Context.Settings.Proctoring.Enabled)
			{
				logger.Info("Initializing proctoring...");
				StatusChanged?.Invoke(TextKey.OperationStatus_InitializeProctoring);

				controller.Initialize(Context.Settings.Proctoring);
				actionCenter.AddNotificationControl(uiFactory.CreateNotificationControl(notification, Location.ActionCenter));

				if (Context.Settings.SessionMode == SessionMode.Server && Context.Settings.Proctoring.ShowRaiseHandNotification)
				{
					actionCenter.AddNotificationControl(uiFactory.CreateRaiseHandControl(controller, Location.ActionCenter, Context.Settings.Proctoring));
					taskbar.AddNotificationControl(uiFactory.CreateRaiseHandControl(controller, Location.Taskbar, Context.Settings.Proctoring));
				}

				if (Context.Settings.Proctoring.ShowTaskbarNotification)
				{
					taskbar.AddNotificationControl(uiFactory.CreateNotificationControl(notification, Location.Taskbar));
				}
			}

			return OperationResult.Success;
		}

		public override OperationResult Revert()
		{
			if (Context.Settings.Proctoring.Enabled)
			{
				logger.Info("Terminating proctoring...");
				StatusChanged?.Invoke(TextKey.OperationStatus_TerminateProctoring);

				controller.Terminate();
				notification.Terminate();
			}

			return OperationResult.Success;
		}
	}
}
