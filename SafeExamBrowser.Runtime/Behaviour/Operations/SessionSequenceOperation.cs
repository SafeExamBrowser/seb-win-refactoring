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
				service.StopSession(session.Id);
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
			}
		}

		protected void StopSession()
		{
			if (sessionRunning)
			{
				logger.Info($"Stopping session with identifier '{session.Id}'...");
				ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_StopSession, true);

				service.StopSession(session.Id);

				// TODO:
				// - Terminate client (or does it terminate itself?)

				sessionRunning = false;
				logger.Info($"Successfully stopped session with identifier '{session.Id}'.");
			}
		}

		private void StartClient()
		{
			const int TEN_SECONDS = 10000;

			var clientReady = new AutoResetEvent(false);
			var clientReadyHandler = new CommunicationEventHandler(() => clientReady.Set());
			var clientExecutable = configuration.RuntimeInfo.ClientExecutablePath;
			var clientLogFile = $"{'"' + configuration.RuntimeInfo.ClientLogFile + '"'}";
			var hostUri = configuration.RuntimeInfo.RuntimeAddress;
			var token = session.StartupToken.ToString("D");

			runtimeHost.ClientReady += clientReadyHandler;
			session.ClientProcess = processFactory.StartNew(clientExecutable, clientLogFile, hostUri, token);

			var clientStarted = clientReady.WaitOne(TEN_SECONDS);

			runtimeHost.ClientReady -= clientReadyHandler;

			// TODO: Check if client process alive!
			if (clientStarted)
			{
				if (client.Connect(session.StartupToken))
				{
					var response = client.RequestAuthentication();

					// TODO: Further integrity checks necessary?
					if (session.ClientProcess.Id == response.ProcessId)
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
	}
}
