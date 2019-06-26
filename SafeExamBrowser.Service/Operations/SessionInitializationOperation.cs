/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading;
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Service.Operations
{
	internal class SessionInitializationOperation : SessionOperation
	{
		private ILogger logger;
		private Func<string, ILogObserver> logWriterFactory;
		private Func<string, EventWaitHandle> serviceEventFactory;
		private IServiceHost serviceHost;
		private ILogObserver sessionWriter;

		public SessionInitializationOperation(
			ILogger logger,
			Func<string, ILogObserver> logWriterFactory,
			Func<string, EventWaitHandle> serviceEventFactory,
			IServiceHost serviceHost,
			SessionContext sessionContext) : base(sessionContext)
		{
			this.logger = logger;
			this.logWriterFactory = logWriterFactory;
			this.serviceEventFactory = serviceEventFactory;
			this.serviceHost = serviceHost;
		}

		public override OperationResult Perform()
		{
			InitializeSessionWriter();

			logger.Info("Initializing new session...");
			logger.Info($" -> Client-ID: {Context.Configuration.AppConfig.ClientId}");
			logger.Info($" -> Runtime-ID: {Context.Configuration.AppConfig.RuntimeId}");
			logger.Info($" -> Session-ID: {Context.Configuration.SessionId}");

			logger.Info("Stopping auto-restore mechanism...");
			Context.AutoRestoreMechanism.Stop();

			logger.Info("Disabling service host...");
			serviceHost.AllowConnection = false;

			InitializeServiceEvent();

			return OperationResult.Success;
		}

		public override OperationResult Revert()
		{
			var success = true;

			logger.Info("Finalizing current session...");

			if (Context.ServiceEvent != null)
			{
				success = Context.ServiceEvent.Set();

				if (success)
				{
					logger.Info("Successfully informed runtime about session termination.");
				}
				else
				{
					logger.Error("Failed to inform runtime about session termination!");
				}
			}

			logger.Info("Starting auto-restore mechanism...");
			Context.AutoRestoreMechanism.Start();

			logger.Info("Enabling service host...");
			serviceHost.AllowConnection = true;

			logger.Info("Clearing session data...");
			Context.Configuration = null;

			FinalizeSessionWriter();

			return success ? OperationResult.Success : OperationResult.Failed;
		}

		private void InitializeServiceEvent()
		{
			if (Context.ServiceEvent != null)
			{
				logger.Info("Closing service event from previous session...");
				Context.ServiceEvent.Close();
				logger.Info("Service event successfully closed.");
			}

			logger.Info("Attempting to create new service event...");
			Context.ServiceEvent = serviceEventFactory.Invoke(Context.Configuration.AppConfig.ServiceEventName);
			logger.Info("Service event successfully created.");
		}

		private void InitializeSessionWriter()
		{
			sessionWriter = logWriterFactory.Invoke(Context.Configuration.AppConfig.ServiceLogFilePath);
			logger.Subscribe(sessionWriter);
			logger.Debug($"Created session log file {Context.Configuration.AppConfig.ServiceLogFilePath}.");
		}

		private void FinalizeSessionWriter()
		{
			logger.Debug("Closed session log file.");
			logger.Unsubscribe(sessionWriter);
			sessionWriter = null;
		}
	}
}
