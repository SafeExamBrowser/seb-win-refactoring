/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Server.Contracts;
using SafeExamBrowser.Settings;
using SafeExamBrowser.SystemComponents.Contracts;
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
		private readonly ISystemInfo systemInfo;
		private readonly ITaskbar taskbar;
		private readonly IUserInterfaceFactory uiFactory;

		private AppConfig AppConfig => Context.AppConfig;

		public override event StatusChangedEventHandler StatusChanged;

		public ServerOperation(
			IActionCenter actionCenter,
			ClientContext context,
			IInvigilator invigilator,
			ILogger logger,
			IServerProxy server,
			ISystemInfo systemInfo,
			ITaskbar taskbar,
			IUserInterfaceFactory uiFactory) : base(context)
		{
			this.actionCenter = actionCenter;
			this.invigilator = invigilator;
			this.logger = logger;
			this.server = server;
			this.systemInfo = systemInfo;
			this.taskbar = taskbar;
			this.uiFactory = uiFactory;
		}

		public override OperationResult Perform()
		{
			if (Context.Settings.SessionMode == SessionMode.Server)
			{
				logger.Info("Initializing server...");
				StatusChanged?.Invoke(TextKey.OperationStatus_InitializeServer);

				server.Initialize(AppConfig.ServerApi, AppConfig.ServerConnectionToken, AppConfig.ServerExamId, AppConfig.ServerOauth2Token, Context.Settings.Server);
				server.StartConnectivity();

				LogRuntimeInformation();

				if (Context.Settings.Server.Invigilation.ShowRaiseHandNotification)
				{
					invigilator.Initialize(Context.Settings.Server.Invigilation);

					actionCenter.AddNotificationControl(uiFactory.CreateRaiseHandControl(invigilator, Location.ActionCenter, Context.Settings.Server.Invigilation));
					taskbar.AddNotificationControl(uiFactory.CreateRaiseHandControl(invigilator, Location.Taskbar, Context.Settings.Server.Invigilation));
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

		private void LogRuntimeInformation()
		{
			logger.Info($"Machine Information: Computer '{systemInfo.Name}' is a {systemInfo.Model} manufactured by {systemInfo.Manufacturer}.");
			logger.Info($"System Information: Running on {systemInfo.OperatingSystemInfo}.");
			logger.Info($"Version Information: {AppConfig.ProgramTitle}, Version {AppConfig.ProgramInformationalVersion}, Build {AppConfig.ProgramBuildVersion}.");
		}
	}
}
