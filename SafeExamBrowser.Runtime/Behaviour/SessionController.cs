/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Threading;
using SafeExamBrowser.Contracts.Behaviour.OperationModel;
using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.WindowsApi;

namespace SafeExamBrowser.Runtime.Behaviour
{
	internal class SessionController
	{
		private const int TEN_SECONDS = 10000;

		private bool sessionRunning;
		private IConfigurationRepository configuration;
		private ILogger logger;
		private IProcessFactory processFactory;
		private IProxyFactory proxyFactory;
		private IRuntimeHost runtimeHost;
		private IServiceProxy service;

		internal IProgressIndicator ProgressIndicator { private get; set; }

		internal SessionController(
			IConfigurationRepository configuration,
			ILogger logger,
			IProcessFactory processFactory,
			IProxyFactory proxyFactory,
			IRuntimeHost runtimeHost,
			IServiceProxy service)
		{
			this.configuration = configuration;
			this.logger = logger;
			this.processFactory = processFactory;
			this.proxyFactory = proxyFactory;
			this.runtimeHost = runtimeHost;
			this.service = service;
		}

		internal OperationResult StartSession()
		{
			logger.Info("Starting new session...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_StartSession, true);

			InitializeSessionConfiguration();
			StartServiceSession();
			sessionRunning = TryStartClient();

			if (!sessionRunning)
			{
				logger.Info($"Failed to start new session! Reverting service session and aborting procedure...");
				service.StopSession(configuration.CurrentSession.Id);

				return OperationResult.Failed;
			}

			logger.Info($"Successfully started new session.");

			return OperationResult.Success;
		}

		internal OperationResult StopSession()
		{
			if (sessionRunning)
			{
				logger.Info($"Stopping session with identifier '{configuration.CurrentSession.Id}'...");
				ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_StopSession, true);

				StopServiceSession();

				if (!configuration.CurrentSession.ClientProcess.HasTerminated)
				{
					StopClient();
				}

				sessionRunning = false;
				logger.Info($"Successfully stopped session.");
			}

			return OperationResult.Success;
		}

		private void InitializeSessionConfiguration()
		{
			configuration.InitializeSessionConfiguration();
			runtimeHost.StartupToken = configuration.CurrentSession.StartupToken;

			logger.Info($" -> Client-ID: {configuration.RuntimeInfo.ClientId}");
			logger.Info($" -> Runtime-ID: {configuration.RuntimeInfo.RuntimeId} (as reference, does not change)");
			logger.Info($" -> Session-ID: {configuration.CurrentSession.Id}");
		}

		private void StartServiceSession()
		{
			logger.Info("Initializing service session...");
			service.StartSession(configuration.CurrentSession.Id, configuration.CurrentSettings);
		}

		private void StopServiceSession()
		{
			logger.Info("Stopping service session...");
			service.StopSession(configuration.CurrentSession.Id);
		}

		private bool TryStartClient()
		{
			var clientReady = false;
			var clientReadyEvent = new AutoResetEvent(false);
			var clientReadyEventHandler = new CommunicationEventHandler(() => clientReadyEvent.Set());
			var clientExecutable = configuration.RuntimeInfo.ClientExecutablePath;
			var clientLogFile = $"{'"' + configuration.RuntimeInfo.ClientLogFile + '"'}";
			var hostUri = configuration.RuntimeInfo.RuntimeAddress;
			var token = configuration.CurrentSession.StartupToken.ToString("D");

			logger.Info("Starting new client process...");
			runtimeHost.ClientReady += clientReadyEventHandler;
			configuration.CurrentSession.ClientProcess = processFactory.StartNew(clientExecutable, clientLogFile, hostUri, token);

			logger.Info("Waiting for client to complete initialization...");
			clientReady = clientReadyEvent.WaitOne(TEN_SECONDS);
			runtimeHost.ClientReady -= clientReadyEventHandler;

			if (!clientReady)
			{
				logger.Error($"Failed to start client within {TEN_SECONDS / 1000} seconds!");

				return false;
			}

			logger.Info("Client has been successfully started and initialized.");
			logger.Info("Creating communication proxy for client host...");
			configuration.CurrentSession.ClientProxy = proxyFactory.CreateClientProxy(configuration.RuntimeInfo.ClientAddress);

			if (!configuration.CurrentSession.ClientProxy.Connect(configuration.CurrentSession.StartupToken))
			{
				logger.Error("Failed to connect to client!");

				return false;
			}

			logger.Info("Connection with client has been established.");

			var response = configuration.CurrentSession.ClientProxy.RequestAuthentication();

			if (configuration.CurrentSession.ClientProcess.Id != response?.ProcessId)
			{
				logger.Error("Failed to verify client integrity!");

				return false;
			}

			logger.Info("Authentication of client has been successful, client is ready to operate.");

			return true;
		}

		private void StopClient()
		{
			var disconnected = false;
			var disconnectedEvent = new AutoResetEvent(false);
			var disconnectedEventHandler = new CommunicationEventHandler(() => disconnectedEvent.Set());

			var terminated = false;
			var terminatedEvent = new AutoResetEvent(false);
			var terminatedEventHandler = new ProcessTerminatedEventHandler((_) => terminatedEvent.Set());

			runtimeHost.ClientDisconnected += disconnectedEventHandler;
			configuration.CurrentSession.ClientProcess.Terminated += terminatedEventHandler;

			logger.Info("Instructing client to initiate shutdown procedure.");
			configuration.CurrentSession.ClientProxy.InitiateShutdown();

			logger.Info("Disconnecting from client communication host.");
			configuration.CurrentSession.ClientProxy.Disconnect();

			logger.Info("Waiting for client to disconnect from runtime communication host...");
			disconnected = disconnectedEvent.WaitOne(TEN_SECONDS);

			if (!disconnected)
			{
				logger.Error($"Client failed to disconnect within {TEN_SECONDS / 1000} seconds!");
			}

			logger.Info("Waiting for client process to terminate...");
			terminated = terminatedEvent.WaitOne(TEN_SECONDS);

			if (!terminated)
			{
				logger.Error($"Client failed to terminate within {TEN_SECONDS / 1000} seconds!");
			}

			runtimeHost.ClientDisconnected -= disconnectedEventHandler;
			configuration.CurrentSession.ClientProcess.Terminated -= terminatedEventHandler;

			if (disconnected && terminated)
			{
				logger.Info("Client has been successfully terminated.");
			}
			else
			{
				logger.Warn("Attempting to kill client process since graceful termination failed!");
				KillClient();
			}
		}

		private void KillClient(int attempt = 0)
		{
			const int MAX_ATTEMPTS = 5;

			if (attempt == MAX_ATTEMPTS)
			{
				logger.Error($"Failed to kill client process within {MAX_ATTEMPTS} attempts!");

				return;
			}

			logger.Info($"Killing client process with ID = {configuration.CurrentSession.ClientProcess.Id}.");
			configuration.CurrentSession.ClientProcess.Kill();

			if (configuration.CurrentSession.ClientProcess.HasTerminated)
			{
				logger.Info("Client process has terminated.");
			}
			else
			{
				logger.Warn("Failed to kill client process. Trying again...");
				KillClient(attempt++);
			}
		}
	}
}
