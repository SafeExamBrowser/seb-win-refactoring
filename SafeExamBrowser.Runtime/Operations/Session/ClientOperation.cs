/*
 * Copyright (c) 2025 ETH Zürich, IT Services
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
using SafeExamBrowser.WindowsApi.Contracts;
using SafeExamBrowser.WindowsApi.Contracts.Events;

namespace SafeExamBrowser.Runtime.Operations.Session
{
	internal class ClientOperation : SessionOperation
	{
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

		public override event StatusChangedEventHandler StatusChanged;

		public ClientOperation(
			Dependencies dependencies,
			IProcessFactory processFactory,
			IProxyFactory proxyFactory,
			IRuntimeHost runtimeHost,
			int timeout_ms) : base(dependencies)
		{
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
				Logger.Info("Successfully started new client instance.");
			}
			else
			{
				Logger.Error("Failed to start new client instance! Aborting procedure...");
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
			var success = false;

			var authenticationToken = Context.Next.ClientAuthenticationToken.ToString("D");
			var executablePath = Context.Next.AppConfig.ClientExecutablePath;
			var logFilePath = $"{'"' + Convert.ToBase64String(Encoding.UTF8.GetBytes(Context.Next.AppConfig.ClientLogFilePath)) + '"'}";
			var logLevel = Context.Next.Settings.LogLevel.ToString();
			var runtimeHostUri = Context.Next.AppConfig.RuntimeAddress;
			var uiMode = Context.Next.Settings.UserInterface.Mode.ToString();

			var clientReady = false;
			var clientReadyEvent = new AutoResetEvent(false);
			var clientReadyEventHandler = new CommunicationEventHandler(() => clientReadyEvent.Set());

			var clientTerminated = false;
			var clientTerminatedEventHandler = new ProcessTerminatedEventHandler(_ => { clientTerminated = true; clientReadyEvent.Set(); });

			Logger.Info("Starting new client process...");
			runtimeHost.AllowConnection = true;
			runtimeHost.AuthenticationToken = Context.Next.ClientAuthenticationToken;
			runtimeHost.ClientReady += clientReadyEventHandler;
			ClientProcess = processFactory.StartNew(executablePath, logFilePath, logLevel, runtimeHostUri, authenticationToken, uiMode);
			ClientProcess.Terminated += clientTerminatedEventHandler;

			Logger.Info("Waiting for client to complete initialization...");
			clientReady = clientReadyEvent.WaitOne();

			runtimeHost.AllowConnection = false;
			runtimeHost.AuthenticationToken = default;
			runtimeHost.ClientReady -= clientReadyEventHandler;
			ClientProcess.Terminated -= clientTerminatedEventHandler;

			if (clientReady && !clientTerminated)
			{
				success = TryStartCommunication();
			}
			else
			{
				Logger.Error("Client instance terminated unexpectedly during initialization!");
			}

			return success;
		}

		private bool TryStartCommunication()
		{
			var success = false;

			Logger.Info("Client has been successfully started and initialized. Creating communication proxy for client host...");
			ClientProxy = proxyFactory.CreateClientProxy(Context.Next.AppConfig.ClientAddress, Interlocutor.Runtime);

			if (ClientProxy.Connect(Context.Next.ClientAuthenticationToken))
			{
				Logger.Info("Connection with client has been established. Requesting authentication...");

				var communication = ClientProxy.RequestAuthentication();
				var response = communication.Value;

				success = communication.Success && ClientProcess.Id == response?.ProcessId;

				if (success)
				{
					Logger.Info("Authentication of client has been successful, client is ready to operate.");
				}
				else
				{
					Logger.Error("Failed to verify client integrity!");
				}
			}
			else
			{
				Logger.Error("Failed to connect to client!");
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

				Logger.Info("Instructing client to initiate shutdown procedure.");
				ClientProxy.InitiateShutdown();

				Logger.Info("Disconnecting from client communication host.");
				ClientProxy.Disconnect();

				Logger.Info("Waiting for client to disconnect from runtime communication host...");
				disconnected = disconnectedEvent.WaitOne(timeout_ms / 2);

				if (!disconnected)
				{
					Logger.Error($"Client failed to disconnect within {timeout_ms / 2 / 1000} seconds!");
				}

				Logger.Info("Waiting for client process to terminate...");
				terminated = terminatedEvent.WaitOne(timeout_ms / 2);

				if (!terminated)
				{
					Logger.Error($"Client failed to terminate within {timeout_ms / 2 / 1000} seconds!");
				}

				runtimeHost.ClientDisconnected -= disconnectedEventHandler;
				ClientProcess.Terminated -= terminatedEventHandler;
			}

			if (disconnected && terminated)
			{
				Logger.Info("Client has been successfully terminated.");
				success = true;
			}
			else
			{
				Logger.Warn("Attempting to kill client process since graceful termination failed!");
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
				Logger.Info($"Attempt {attempt}/{MAX_ATTEMPTS} to kill client process with ID = {ClientProcess.Id}.");

				if (ClientProcess.TryKill(500))
				{
					break;
				}
			}

			if (ClientProcess.HasTerminated)
			{
				Logger.Info("Client process has terminated.");
			}
			else
			{
				Logger.Error($"Failed to kill client process within {MAX_ATTEMPTS} attempts!");
			}

			return ClientProcess.HasTerminated;
		}
	}
}
