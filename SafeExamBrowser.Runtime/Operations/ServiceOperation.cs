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
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Core.OperationModel.Events;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class ServiceOperation : SessionOperation
	{
		private ILogger logger;
		private IRuntimeHost runtimeHost;
		private IServiceProxy service;
		private int timeout_ms;

		public override event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public override event StatusChangedEventHandler StatusChanged;

		public ServiceOperation(
			ILogger logger,
			IRuntimeHost runtimeHost,
			IServiceProxy service,
			SessionContext sessionContext,
			int timeout_ms) : base(sessionContext)
		{
			this.logger = logger;
			this.runtimeHost = runtimeHost;
			this.service = service;
			this.timeout_ms = timeout_ms;
		}

		public override OperationResult Perform()
		{
			logger.Info($"Initializing service session...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeServiceSession);

			var success = TryEstablishConnection();

			if (service.IsConnected)
			{
				success = TryStartSession();
			}

			return success ? OperationResult.Success : OperationResult.Failed;
		}

		public override OperationResult Repeat()
		{
			logger.Info($"Initializing new service session...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeServiceSession);

			var success = false;

			if (service.IsConnected)
			{
				success = TryStopSession();
			}
			else
			{
				success = TryEstablishConnection();
			}

			if (success && service.IsConnected)
			{
				success = TryStartSession();
			}

			return success ? OperationResult.Success : OperationResult.Failed;
		}

		public override OperationResult Revert()
		{
			logger.Info("Finalizing service session...");
			StatusChanged?.Invoke(TextKey.OperationStatus_FinalizeServiceSession);

			var success = true;

			if (service.IsConnected)
			{
				success &= TryStopSession();
				success &= TryTerminateConnection();
			}

			return success ? OperationResult.Success : OperationResult.Failed;
		}

		private bool TryEstablishConnection()
		{
			var mandatory = Context.Next.Settings.ServicePolicy == ServicePolicy.Mandatory;
			var connected = service.Connect();
			var success = connected || !mandatory;

			if (success)
			{
				service.Ignore = !connected;
				logger.Info($"The service is {(mandatory ? "mandatory" : "optional")} and {(connected ? "connected." : "not connected. All service-related operations will be ignored!")}");
			}
			else
			{
				logger.Error("The service is mandatory but no connection could be established!");
			}

			return success;
		}

		private bool TryTerminateConnection()
		{
			var serviceEvent = new AutoResetEvent(false);
			var serviceEventHandler = new CommunicationEventHandler(() => serviceEvent.Set());

			runtimeHost.ServiceDisconnected += serviceEventHandler;

			var success = service.Disconnect();

			if (success)
			{
				logger.Info("Successfully disconnected from service. Waiting for service to disconnect...");

				success = serviceEvent.WaitOne(timeout_ms);

				if (success)
				{
					logger.Info("Service disconnected successfully.");
				}
				else
				{
					logger.Error($"Service failed to disconnect within {timeout_ms / 1000} seconds!");
				}
			}
			else
			{
				logger.Error("Failed to disconnect from service!");
			}

			runtimeHost.ServiceDisconnected -= serviceEventHandler;

			return success;
		}

		private bool TryStartSession()
		{
			var failure = false;
			var success = false;
			var serviceEvent = new AutoResetEvent(false);
			var failureEventHandler = new CommunicationEventHandler(() => { failure = true; serviceEvent.Set(); });
			var successEventHandler = new CommunicationEventHandler(() => { success = true; serviceEvent.Set(); });

			runtimeHost.ServiceFailed += failureEventHandler;
			runtimeHost.ServiceSessionStarted += successEventHandler;

			logger.Info("Starting new service session...");

			var communication = service.StartSession(Context.Next.Id, Context.Next.Settings);

			if (communication.Success)
			{
				serviceEvent.WaitOne(timeout_ms);

				if (success)
				{
					logger.Info("Successfully started new service session.");
				}
				else if (failure)
				{
					logger.Error("An error occurred while attempting to start a new service session! Please check the service log for further information.");
				}
				else
				{
					logger.Error($"Failed to start new service session within {timeout_ms / 1000} seconds!");
				}
			}
			else
			{
				logger.Error("Failed to communicate session start to service!");
			}

			runtimeHost.ServiceFailed -= failureEventHandler;
			runtimeHost.ServiceSessionStarted -= successEventHandler;

			return success;
		}

		private bool TryStopSession()
		{
			var failure = false;
			var success = false;
			var serviceEvent = new AutoResetEvent(false);
			var failureEventHandler = new CommunicationEventHandler(() => { failure = true; serviceEvent.Set(); });
			var successEventHandler = new CommunicationEventHandler(() => { success = true; serviceEvent.Set(); });

			runtimeHost.ServiceFailed += failureEventHandler;
			runtimeHost.ServiceSessionStopped += successEventHandler;

			logger.Info("Stopping current service session...");

			var communication = service.StopSession(Context.Current.Id);

			if (communication.Success)
			{
				serviceEvent.WaitOne(timeout_ms);

				if (success)
				{
					logger.Info("Successfully stopped service session.");
				}
				else if (failure)
				{
					logger.Error("An error occurred while attempting to stop the current service session! Please check the service log for further information.");
				}
				else
				{
					logger.Error($"Failed to stop service session within {timeout_ms / 1000} seconds!");
				}
			}
			else
			{
				logger.Error("Failed to communicate session stop to service!");
			}

			runtimeHost.ServiceFailed -= failureEventHandler;
			runtimeHost.ServiceSessionStopped -= successEventHandler;

			return success;
		}
	}
}
