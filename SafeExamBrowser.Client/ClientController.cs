/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Client.Responsibilities;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.ResponsibilityModel;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Client
{
	internal class ClientController
	{
		private readonly ClientContext context;
		private readonly ILogger logger;
		private readonly IOperationSequence operations;
		private readonly IResponsibilityCollection<ClientTask> responsibilities;
		private readonly IRuntimeProxy runtime;
		private readonly ISplashScreen splashScreen;

		internal ClientController(
			ClientContext context,
			ILogger logger,
			IOperationSequence operations,
			IResponsibilityCollection<ClientTask> responsibilities,
			IRuntimeProxy runtime,
			ISplashScreen splashScreen)
		{
			this.context = context;
			this.logger = logger;
			this.operations = operations;
			this.responsibilities = responsibilities;
			this.runtime = runtime;
			this.splashScreen = splashScreen;
		}

		internal bool TryStart()
		{
			logger.Info("Initiating startup procedure...");

			splashScreen.Show();
			splashScreen.BringToForeground();

			var initialized = false;
			var result = operations.TryPerform();

			if (result == OperationResult.Success)
			{
				DelegateStartupResponsibilities();
				initialized = TryInformRuntime();
			}
			else
			{
				logger.Info($"Application startup {(result == OperationResult.Aborted ? "aborted" : "failed")}!");
				logger.Log(string.Empty);
			}

			if (initialized)
			{
				logger.Info("Application successfully initialized.");
				logger.Log(string.Empty);

				DelegateClientReadyResponsibilities();
			}

			splashScreen.Hide();

			return initialized;
		}

		internal void Terminate()
		{
			logger.Log(string.Empty);
			logger.Info("Initiating shutdown procedure...");

			splashScreen.Show();
			splashScreen.BringToForeground();

			DelegateShutdownResponsibilities();

			var success = operations.TryRevert() == OperationResult.Success;

			if (success)
			{
				logger.Info("Application successfully finalized.");
				logger.Log(string.Empty);
			}
			else
			{
				logger.Info("Application shutdown failed!");
				logger.Log(string.Empty);
			}

			splashScreen.Close();
		}

		internal void UpdateAppConfig()
		{
			splashScreen.AppConfig = context.AppConfig;
		}

		private void DelegateStartupResponsibilities()
		{
			responsibilities.Delegate(ClientTask.RegisterEvents);
			responsibilities.Delegate(ClientTask.ShowShell);
			responsibilities.Delegate(ClientTask.AutoStartApplications);
			responsibilities.Delegate(ClientTask.ScheduleIntegrityVerification);
			responsibilities.Delegate(ClientTask.StartMonitoring);
		}

		private void DelegateClientReadyResponsibilities()
		{
			responsibilities.Delegate(ClientTask.VerifySessionIntegrity);
		}

		private void DelegateShutdownResponsibilities()
		{
			responsibilities.Delegate(ClientTask.CloseShell);
			responsibilities.Delegate(ClientTask.DeregisterEvents);
			responsibilities.Delegate(ClientTask.UpdateSessionIntegrity);
		}

		private bool TryInformRuntime()
		{
			var communication = runtime.InformClientReady();

			if (!communication.Success)
			{
				logger.Error("Failed to inform runtime that client is ready!");
			}

			return communication.Success;
		}
	}
}
