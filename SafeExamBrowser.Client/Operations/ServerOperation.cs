/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
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
using SafeExamBrowser.UserInterface.Contracts.Shell;

namespace SafeExamBrowser.Client.Operations
{
	internal class ServerOperation : ClientOperation
	{
		private readonly IActionCenter actionCenter;
		private readonly ILogger logger;
		private readonly IServerProxy server;
		private readonly ITaskbar taskbar;

		public override event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public override event StatusChangedEventHandler StatusChanged;

		public ServerOperation(
			IActionCenter actionCenter,
			ClientContext context,
			ILogger logger,
			IServerProxy server,
			ITaskbar taskbar) : base(context)
		{
			this.actionCenter = actionCenter;
			this.logger = logger;
			this.server = server;
			this.taskbar = taskbar;
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

				if (Context.AppConfig.ServerConnectivityAutoStart)
				{
					server.StartConnectivity();
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
