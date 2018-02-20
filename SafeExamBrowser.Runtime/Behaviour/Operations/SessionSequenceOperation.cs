/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
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

		public bool Abort { get; private set; }
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

		public abstract void Perform();
		public abstract void Repeat();
		public abstract void Revert();

		protected void StartSession()
		{
			logger.Info("Starting new session...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_StartSession, true);

			session = configuration.InitializeSession();
			runtimeHost.StartupToken = session.StartupToken;

			service.StartSession(session.Id, configuration.CurrentSettings);

			try
			{
				StartClient();
			}
			catch (Exception e)
			{
				logger.Error("Failed to start client!", e);
			}

			if (sessionRunning)
			{
				logger.Info($"Successfully started new session with identifier '{session.Id}'.");
			}
			else
			{
				Abort = true;
				logger.Info($"Failed to start new session! Aborting...");
				service.StopSession(session.Id);
			}
		}

		protected void StopSession()
		{
			if (sessionRunning)
			{
				logger.Info($"Stopping session with identifier '{session.Id}'...");
				ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_StopSession, true);

				service.StopSession(session.Id);

				try
				{
					StopClient();
				}
				catch (Exception e)
				{
					logger.Error("Failed to terminate client!", e);
				}

				sessionRunning = false;
				logger.Info($"Successfully stopped session with identifier '{session.Id}'.");
			}
		}

		private void StartClient()
		{
			var clientReady = false;
			var clientReadyEvent = new AutoResetEvent(false);
			var clientReadyEventHandler = new CommunicationEventHandler(() => clientReadyEvent.Set());
			var clientExecutable = configuration.RuntimeInfo.ClientExecutablePath;
			var clientLogFile = $"{'"' + configuration.RuntimeInfo.ClientLogFile + '"'}";
			var hostUri = configuration.RuntimeInfo.RuntimeAddress;
			var token = session.StartupToken.ToString("D");

			runtimeHost.ClientReady += clientReadyEventHandler;

			session.ClientProcess = processFactory.StartNew(clientExecutable, clientLogFile, hostUri, token);
			clientReady = clientReadyEvent.WaitOne(TEN_SECONDS);

			runtimeHost.ClientReady -= clientReadyEventHandler;

			// TODO: Check if client process alive!
			if (clientReady)
			{
				if (client.Connect(session.StartupToken))
				{
					var response = client.RequestAuthentication();

					// TODO: Further integrity checks necessary?
					if (session.ClientProcess.Id == response?.ProcessId)
					{
						sessionRunning = true;
					}
					else
					{
						logger.Error("Failed to verify client integrity!");
					}
				}
				else
				{
					logger.Error("Failed to connect to client!");
				}
			}
			else
			{
				logger.Error($"Failed to start client within {TEN_SECONDS / 1000} seconds!");
			}
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

			client.InitiateShutdown();
			client.Disconnect();

			disconnected = disconnectedEvent.WaitOne(TEN_SECONDS);
			terminated = terminatedEvent.WaitOne(TEN_SECONDS);

			runtimeHost.ClientDisconnected -= disconnectedEventHandler;
			session.ClientProcess.Terminated -= terminatedEventHandler;

			if (disconnected && terminated)
			{
				logger.Info("Client has been successfully terminated.");
			}
			else
			{
				if (!disconnected)
				{
					logger.Error($"Client failed to disconnect within {TEN_SECONDS / 1000} seconds!");
				}
				else if (!terminated)
				{
					logger.Error($"Client failed to terminate within {TEN_SECONDS / 1000} seconds!");
				}

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
