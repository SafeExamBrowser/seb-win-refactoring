/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Threading;
using SafeExamBrowser.Contracts.Communication.Events;
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Core.OperationModel.Events;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.WindowsApi;
using SafeExamBrowser.Contracts.WindowsApi.Events;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class ClientOperation : SessionOperation
	{
		private readonly int timeout_ms;

		private ILogger logger;
		private IProcessFactory processFactory;
		private IProxyFactory proxyFactory;
		private IRuntimeHost runtimeHost;

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
			var clientReady = false;
			var clientReadyEvent = new AutoResetEvent(false);
			var clientReadyEventHandler = new CommunicationEventHandler(() => clientReadyEvent.Set());

			var clientExecutable = Context.Next.AppConfig.ClientExecutablePath;
			var clientLogFile = $"{'"' + Context.Next.AppConfig.ClientLogFile + '"'}";
			var clientLogLevel = Context.Next.Settings.LogLevel.ToString();
			var runtimeHostUri = Context.Next.AppConfig.RuntimeAddress;
			var startupToken = Context.Next.StartupToken.ToString("D");
			var uiMode = Context.Next.Settings.UserInterfaceMode.ToString();

			logger.Info("Starting new client process...");
			runtimeHost.AllowConnection = true;
			runtimeHost.ClientReady += clientReadyEventHandler;
			ClientProcess = processFactory.StartNew(clientExecutable, clientLogFile, clientLogLevel, runtimeHostUri, startupToken, uiMode);

			logger.Info("Waiting for client to complete initialization...");
			clientReady = clientReadyEvent.WaitOne(timeout_ms);
			runtimeHost.ClientReady -= clientReadyEventHandler;
			runtimeHost.AllowConnection = false;

			if (!clientReady)
			{
				logger.Error($"Failed to start client within {timeout_ms / 1000} seconds!");

				return false;
			}

			logger.Info("Client has been successfully started and initialized. Creating communication proxy for client host...");
			ClientProxy = proxyFactory.CreateClientProxy(Context.Next.AppConfig.ClientAddress);

			if (!ClientProxy.Connect(Context.Next.StartupToken))
			{
				logger.Error("Failed to connect to client!");

				return false;
			}

			logger.Info("Connection with client has been established. Requesting authentication...");

			var communication = ClientProxy.RequestAuthentication();
			var response = communication.Value;

			if (!communication.Success || ClientProcess.Id != response?.ProcessId)
			{
				logger.Error("Failed to verify client integrity!");

				return false;
			}

			logger.Info("Authentication of client has been successful, client is ready to operate.");

			return true;
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
				disconnected = disconnectedEvent.WaitOne(timeout_ms);

				if (!disconnected)
				{
					logger.Error($"Client failed to disconnect within {timeout_ms / 1000} seconds!");
				}

				logger.Info("Waiting for client process to terminate...");
				terminated = terminatedEvent.WaitOne(timeout_ms);

				if (!terminated)
				{
					logger.Error($"Client failed to terminate within {timeout_ms / 1000} seconds!");
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

		private bool TryKillClient(int attempt = 0)
		{
			const int MAX_ATTEMPTS = 5;

			if (attempt == MAX_ATTEMPTS)
			{
				logger.Error($"Failed to kill client process within {MAX_ATTEMPTS} attempts!");

				return false;
			}

			logger.Info($"Killing client process with ID = {ClientProcess.Id}.");
			ClientProcess.Kill();

			if (ClientProcess.HasTerminated)
			{
				logger.Info("Client process has terminated.");

				return true;
			}
			else
			{
				logger.Warn("Failed to kill client process. Trying again...");

				return TryKillClient(++attempt);
			}
		}
	}
}
