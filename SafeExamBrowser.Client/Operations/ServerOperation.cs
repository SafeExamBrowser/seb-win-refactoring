/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Server.Contracts;
using SafeExamBrowser.Settings;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Shell;

namespace SafeExamBrowser.Client.Operations
{
	internal class ServerOperation : ClientOperation
	{
		private readonly IActionCenter actionCenter;
		private readonly IInvigilator invigilator;
		private readonly ILogger logger;
		private readonly IServerProxy server;
		private readonly ITaskbar taskbar;
		private readonly IUserInterfaceFactory uiFactory;

		public override event StatusChangedEventHandler StatusChanged;

		public ServerOperation(
			IActionCenter actionCenter,
			ClientContext context,
			IInvigilator invigilator,
			ILogger logger,
			IServerProxy server,
			ITaskbar taskbar,
			IUserInterfaceFactory uiFactory) : base(context)
		{
			this.actionCenter = actionCenter;
			this.invigilator = invigilator;
			this.logger = logger;
			this.server = server;
			this.taskbar = taskbar;
			this.uiFactory = uiFactory;
		}

		public override OperationResult Perform()
		{
			if (Context.Settings.SessionMode == SessionMode.Server)
			{
				logger.Info("Initializing server...");
				StatusChanged?.Invoke(TextKey.OperationStatus_InitializeServer);

				server.Initialize(
					Context.AppConfig.ServerApi,
					Context.AppConfig.ServerConnectionToken,
					Context.AppConfig.ServerExamId,
					Context.AppConfig.ServerOauth2Token,
					Context.Settings.Server);
				server.StartConnectivity();

				if (Context.Settings.Server.ShowRaiseHandNotification)
				{
					invigilator.Initialize(Context.Settings.Server);

					actionCenter.AddNotificationControl(uiFactory.CreateRaiseHandControl(invigilator, Location.ActionCenter, Context.Settings.Server));
					taskbar.AddNotificationControl(uiFactory.CreateRaiseHandControl(invigilator, Location.Taskbar, Context.Settings.Server));
				}
			}

			return OperationResult.Success;
		}

		public override OperationResult Revert()
		{
			if (Context.Settings?.SessionMode == SessionMode.Server)
			{
				logger.Info("Finalizing server...");
				StatusChanged?.Invoke(TextKey.OperationStatus_FinalizeServer);

				server.StopConnectivity();
			}

			return OperationResult.Success;
		}
	}
}
