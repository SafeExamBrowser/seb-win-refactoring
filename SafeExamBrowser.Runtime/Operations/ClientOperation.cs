/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Text;
using System.Threading;
using SafeExamBrowser.Communication.Contracts;
using SafeExamBrowser.Communication.Contracts.Events;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.WindowsApi.Contracts;
using SafeExamBrowser.WindowsApi.Contracts.Events;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class ClientOperation : SessionOperation
	{
		private readonly ILogger logger;
		private readonly IProcessFactory processFactory;
		private readonly IProxyFactory proxyFactory;
		private readonly IRuntimeHost runtimeHost;
		private readonly int timeout_ms;

		private IProcess ClientProcess
		{
			get { return Context.ClientProcess; }
			set { Context.ClientProcess = value; }
		}

		private IClientProxy ClientProxy
		{
			get { return Context.ClientProxy; }
			set { Context.ClientProxy = value; }
		}

		public override event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public override event StatusChangedEventHandler StatusChanged;

		public ClientOperation(
			ILogger logger,
			IProcessFactory processFactory,
			IProxyFactory proxyFactory,
			IRuntimeHost runtimeHost,
			SessionContext sessionContext,
			int timeout_ms) : base(sessionContext)
		{
			this.logger = logger;
			this.processFactory = processFactory;
			this.proxyFactory = proxyFactory;
			this.runtimeHost = runtimeHost;
			this.timeout_ms = timeout_ms;
		}

		public override OperationResult Perform()
		{
			StatusChanged?.Invoke(TextKey.OperationStatus_StartClient);

			var success = TryStartClient();

			if (success)
			{
				logger.Info("Successfully started new client instance.");
			}
			else
			{
				logger.Error("Failed to start new client instance! Aborting procedure...");
			}

			return success ? OperationResult.Success : OperationResult.Failed;
		}

		public override OperationResult Repeat()
		{
			return Perform();
		}

		public override OperationResult Revert()
		{
			var success = true;

			if (ClientProcess != null && !ClientProcess.HasTerminated)
			{
				StatusChanged?.Invoke(TextKey.OperationStatus_StopClient);
				success = TryStopClient();
			}

			return success ? OperationResult.Success : OperationResult.Failed;
		}

		private bool TryStartClient()
		{
			var authenticationToken = Context.Next.ClientAuthenticationToken.ToString("D");
			var executablePath = Context.Next.AppConfig.ClientExecutablePath;
			var logFilePath = $"{'"' + Convert.ToBase64String(Encoding.UTF8.GetBytes(Context.Next.AppConfig.ClientLogFilePath)) + '"'}";
			var logLevel = Context.Next.Settings.LogLevel.ToString();
			var runtimeHostUri = Context.Next.AppConfig.RuntimeAddress;
			var uiMode = Context.Next.Settings.UserInterfaceMode.ToString();

			var clientReady = false;
			var clientReadyEvent = new AutoResetEvent(false);
			var clientReadyEventHandler = new CommunicationEventHandler(() => clientReadyEvent.Set());

			var clientTerminated = false;
			var clientTerminatedEventHandler = new ProcessTerminatedEventHandler(_ => { clientTerminated = true; clientReadyEvent.Set(); });

			logger.Info("Starting new client process...");
			runtimeHost.AllowConnection = true;
			runtimeHost.AuthenticationToken = Context.Next.ClientAuthenticationToken;
			runtimeHost.ClientReady += clientReadyEventHandler;
			ClientProcess = processFactory.StartNew(executablePath, logFilePath, logLevel, runtimeHostUri, authenticationToken, uiMode);
			ClientProcess.Terminated += clientTerminatedEventHandler;

			logger.Info("Waiting for client to complete initialization...");
			clientReady = clientReadyEvent.WaitOne();

			runtimeHost.AllowConnection = false;
			runtimeHost.AuthenticationToken = default(Guid?);
			runtimeHost.ClientReady -= clientReadyEventHandler;
			ClientProcess.Terminated -= clientTerminatedEventHandler;

			if (clientReady && !clientTerminated)
			{
				return TryStartCommunication();
			}

			if (!clientReady)
			{
				logger.Error($"Failed to start client!");
			}

			if (clientTerminated)
			{
				logger.Error("Client instance terminated unexpectedly during initialization!");
			}

			return false;
		}

		private bool TryStartCommunication()
		{
			var success = false;

			logger.Info("Client has been successfully started and initialized. Creating communication proxy for client host...");
			ClientProxy = proxyFactory.CreateClientProxy(Context.Next.AppConfig.ClientAddress, Interlocutor.Runtime);

			if (ClientProxy.Connect(Context.Next.ClientAuthenticationToken))
			{
				logger.Info("Connection with client has been established. Requesting authentication...");

				var communication = ClientProxy.RequestAuthentication();
				var response = communication.Value;

				success = communication.Success && ClientProcess.Id == response?.ProcessId;

				if (success)
				{
					logger.Info("Authentication of client has been successful, client is ready to operate.");
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

			return success;
		}

		private bool TryStopClient()
		{
			var success = false;

			var disconnected = false;
			var disconnectedEvent = new AutoResetEvent(false);
			var disconnectedEventHandler = new CommunicationEventHandler(() => disconnectedEvent.Set());

			var terminated = false;
			var terminatedEvent = new AutoResetEvent(false);
			var terminatedEventHandler = new ProcessTerminatedEventHandler((_) => terminatedEvent.Set());

			if (ClientProxy != null)
			{
				runtimeHost.ClientDisconnected += disconnectedEventHandler;
				ClientProcess.Terminated += terminatedEventHandler;

				logger.Info("Instructing client to initiate shutdown procedure.");
				ClientProxy.InitiateShutdown();

				logger.Info("Disconnecting from client communication host.");
				ClientProxy.Disconnect();

				logger.Info("Waiting for client to disconnect from runtime communication host...");
				disconnected = disconnectedEvent.WaitOne(timeout_ms / 2);

				if (!disconnected)
				{
					logger.Error($"Client failed to disconnect within {timeout_ms / 2 / 1000} seconds!");
				}

				logger.Info("Waiting for client process to terminate...");
				terminated = terminatedEvent.WaitOne(timeout_ms / 2);

				if (!terminated)
				{
					logger.Error($"Client failed to terminate within {timeout_ms / 2 / 1000} seconds!");
				}

				runtimeHost.ClientDisconnected -= disconnectedEventHandler;
				ClientProcess.Terminated -= terminatedEventHandler;
			}

			if (disconnected && terminated)
			{
				logger.Info("Client has been successfully terminated.");
				success = true;
			}
			else
			{
				logger.Warn("Attempting to kill client process since graceful termination failed!");
				success = TryKillClient();
			}

			if (success)
			{
				ClientProcess = null;
				ClientProxy = null;
			}

			return success;
		}

		private bool TryKillClient()
		{
			const int MAX_ATTEMPTS = 5;

			for (var attempt = 1; attempt <= MAX_ATTEMPTS; attempt++)
			{
				logger.Info($"Attempt {attempt}/{MAX_ATTEMPTS} to kill client process with ID = {ClientProcess.Id}.");

				if (ClientProcess.TryKill(500))
				{
					break;
				}
			}

			if (ClientProcess.HasTerminated)
			{
				logger.Info("Client process has terminated.");
			}
			else
			{
				logger.Error($"Failed to kill client process within {MAX_ATTEMPTS} attempts!");
			}

			return ClientProcess.HasTerminated;
		}
	}
}
