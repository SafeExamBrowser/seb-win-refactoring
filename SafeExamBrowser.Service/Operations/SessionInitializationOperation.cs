/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Service.Operations
{
	internal class SessionInitializationOperation : SessionOperation
	{
		private ILogger logger;
		private Func<string, ILogObserver> createLogWriter;
		private IServiceHost serviceHost;
		private ILogObserver sessionWriter;

		public SessionInitializationOperation(
			ILogger logger,
			Func<string, ILogObserver> createLogWriter,
			IServiceHost serviceHost,
			SessionContext sessionContext) : base(sessionContext)
		{
			this.logger = logger;
			this.createLogWriter = createLogWriter;
			this.serviceHost = serviceHost;
		}

		public override OperationResult Perform()
		{
			InitializeSessionWriter();

			logger.Info("Initializing new session...");

			serviceHost.AllowConnection = false;
			logger.Info($" -> Client-ID: {Context.Configuration.AppConfig.ClientId}");
			logger.Info($" -> Runtime-ID: {Context.Configuration.AppConfig.RuntimeId}");
			logger.Info($" -> Session-ID: {Context.Configuration.SessionId}");

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

			Context.Configuration = null;
			logger.Unsubscribe(sessionWriter);
			sessionWriter = null;
			serviceHost.AllowConnection = true;

			return success ? OperationResult.Success : OperationResult.Failed;
		}

		private void InitializeServiceEvent()
		{
			var securityIdentifier = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
			var accessRule = new EventWaitHandleAccessRule(securityIdentifier, EventWaitHandleRights.Synchronize, AccessControlType.Allow);
			var security = new EventWaitHandleSecurity();

			security.AddAccessRule(accessRule);

			if (Context.ServiceEvent != null)
			{
				logger.Info("Closing service event from previous session...");
				Context.ServiceEvent.Close();
				logger.Info("Service event successfully closed.");
			}

			logger.Info("Attempting to create new service event...");
			Context.ServiceEvent = new EventWaitHandle(false, EventResetMode.AutoReset, Context.Configuration.AppConfig.ServiceEventName, out _, security);
			logger.Info("Service event successfully created.");
		}

		private void InitializeSessionWriter()
		{
			sessionWriter = createLogWriter.Invoke(Context.Configuration.AppConfig.ServiceLogFilePath);
			logger.Subscribe(sessionWriter);
		}
	}
}
