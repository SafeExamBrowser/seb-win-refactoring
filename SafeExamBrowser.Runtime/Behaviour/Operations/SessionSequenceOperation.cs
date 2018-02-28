/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Threading;
using SafeExamBrowser.Contracts.Behaviour.Operations;
using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.WindowsApi;

namespace SafeExamBrowser.Runtime.Behaviour.Operations
{
	internal abstract class SessionSequenceOperation : IOperation
	{
		private const int TEN_SECONDS = 10000;

		private bool sessionRunning;
		private IClientProxy client;
		private IConfigurationRepository configuration;
		private ILogger logger;
		private IProcessFactory processFactory;
		private IRuntimeHost runtimeHost;
		private IServiceProxy service;
		private ISession session;

		public IProgressIndicator ProgressIndicator { private get; set; }

		public SessionSequenceOperation(
			IClientProxy client,
			IConfigurationRepository configuration,
			ILogger logger,
			IProcessFactory processFactory,
			IRuntimeHost runtimeHost,
			IServiceProxy service)
		{
			this.client = client;
			this.configuration = configuration;
			this.logger = logger;
			this.processFactory = processFactory;
			this.runtimeHost = runtimeHost;
			this.service = service;
		}

		public abstract OperationResult Perform();
		public abstract OperationResult Repeat();
		public abstract void Revert();

		protected OperationResult StartSession()
		{
			logger.Info("Starting new session...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_StartSession, true);

			session = configuration.InitializeSession();
			runtimeHost.StartupToken = session.StartupToken;

			logger.Info("Initializing service session...");
			service.StartSession(session.Id, configuration.CurrentSettings);

			sessionRunning = TryStartClient();

			if (!sessionRunning)
			{
				logger.Info($"Failed to start new session! Reverting service session and aborting procedure...");
				service.StopSession(session.Id);

				return OperationResult.Failed;
			}

			logger.Info($"Successfully started new session with identifier '{session.Id}'.");

			return OperationResult.Success;
		}

		protected OperationResult StopSession()
		{
			if (sessionRunning)
			{
				logger.Info($"Stopping session with identifier '{session.Id}'...");
				ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_StopSession, true);

				logger.Info("Stopping service session...");
				service.StopSession(session.Id);

				if (!session.ClientProcess.HasTerminated)
				{
					StopClient();
				}

				sessionRunning = false;
				logger.Info($"Successfully stopped session with identifier '{session.Id}'.");
			}

			return OperationResult.Success;
		}

		private bool TryStartClient()
		{
			var clientReady = false;
			var clientReadyEvent = new AutoResetEvent(false);
			var clientReadyEventHandler = new CommunicationEventHandler(() => clientReadyEvent.Set());
			var clientExecutable = configuration.RuntimeInfo.ClientExecutablePath;
			var clientLogFile = $"{'"' + configuration.RuntimeInfo.ClientLogFile + '"'}";
			var hostUri = configuration.RuntimeInfo.RuntimeAddress;
			var token = session.StartupToken.ToString("D");

			logger.Info("Starting new client process.");
			runtimeHost.ClientReady += clientReadyEventHandler;
			session.ClientProcess = processFactory.StartNew(clientExecutable, clientLogFile, hostUri, token);

			logger.Info("Waiting for client to complete initialization...");
			clientReady = clientReadyEvent.WaitOne(TEN_SECONDS);
			runtimeHost.ClientReady -= clientReadyEventHandler;

			if (!clientReady)
			{
				logger.Error($"Failed to start client within {TEN_SECONDS / 1000} seconds!");

				return false;
			}

			logger.Info("Client has been successfully started and initialized.");

			if (!client.Connect(session.StartupToken))
			{
				logger.Error("Failed to connect to client!");

				return false;
			}

			logger.Info("Connection with client has been established.");

			var response = client.RequestAuthentication();

			if (session.ClientProcess.Id != response?.ProcessId)
			{
				logger.Error("Failed to verify client integrity!");

				return false;
			}

			logger.Info("Authentication of client has been successful.");

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
			session.ClientProcess.Terminated += terminatedEventHandler;

			logger.Info("Instructing client to initiate shutdown procedure.");
			client.InitiateShutdown();

			logger.Info("Disconnecting from client communication host.");
			client.Disconnect();

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
			session.ClientProcess.Terminated -= terminatedEventHandler;

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

			logger.Info($"Killing client process with ID = {session.ClientProcess.Id}.");
			session.ClientProcess.Kill();

			if (session.ClientProcess.HasTerminated)
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
