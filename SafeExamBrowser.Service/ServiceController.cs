/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Communication.Events;
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Service;

namespace SafeExamBrowser.Service
{
	internal class ServiceController : IServiceController
	{
		private readonly ILogger logger;
		private IOperationSequence bootstrapSequence;
		private IOperationSequence sessionSequence;
		private IServiceHost serviceHost;
		private SessionContext sessionContext;

		private ServiceConfiguration Session
		{
			get { return sessionContext.Configuration; }
		}

		private bool SessionIsRunning
		{
			get { return Session != null; }
		}

		public ServiceController(
			ILogger logger,
			IOperationSequence bootstrapSequence,
			IOperationSequence sessionSequence,
			IServiceHost serviceHost,
			SessionContext sessionContext)
		{
			this.logger = logger;
			this.bootstrapSequence = bootstrapSequence;
			this.sessionSequence = sessionSequence;
			this.serviceHost = serviceHost;
			this.sessionContext = sessionContext;
		}

		public bool TryStart()
		{
			logger.Info("Initiating startup procedure...");

			var result = bootstrapSequence.TryPerform();
			var success = result == OperationResult.Success;

			if (success)
			{
				RegisterEvents();

				logger.Info("Service successfully initialized.");
				logger.Log(string.Empty);
			}
			else
			{
				logger.Info("Service startup aborted!");
				logger.Log(string.Empty);
			}

			return success;
		}

		public void Terminate()
		{
			DeregisterEvents();

			if (SessionIsRunning)
			{
				StopSession();
			}

			logger.Log(string.Empty);
			logger.Info("Initiating termination procedure...");

			var result = bootstrapSequence.TryRevert();
			var success = result == OperationResult.Success;

			if (success)
			{
				logger.Info("Service successfully terminated.");
				logger.Log(string.Empty);
			}
			else
			{
				logger.Info("Service termination failed!");
				logger.Log(string.Empty);
			}
		}

		private void StartSession()
		{
			logger.Info(AppendDivider("Session Start Procedure"));

			var result = sessionSequence.TryPerform();

			if (result == OperationResult.Success)
			{
				logger.Info(AppendDivider("Session Running"));
			}
			else
			{
				logger.Info(AppendDivider("Session Start Failed"));
			}
		}

		private void StopSession()
		{
			logger.Info(AppendDivider("Session Stop Procedure"));

			var result = sessionSequence.TryRevert();

			if (result == OperationResult.Success)
			{
				logger.Info(AppendDivider("Session Terminated"));
			}
			else
			{
				logger.Info(AppendDivider("Session Stop Failed"));
			}
		}

		private void RegisterEvents()
		{
			serviceHost.SessionStartRequested += ServiceHost_SessionStartRequested;
			serviceHost.SessionStopRequested += ServiceHost_SessionStopRequested;
		}

		private void DeregisterEvents()
		{
			serviceHost.SessionStartRequested -= ServiceHost_SessionStartRequested;
			serviceHost.SessionStopRequested -= ServiceHost_SessionStopRequested;
		}

		private void ServiceHost_SessionStartRequested(SessionStartEventArgs args)
		{
			if (!SessionIsRunning)
			{
				sessionContext.Configuration = args.Configuration;

				StartSession();
			}
			else
			{
				logger.Warn("Received session start request, even though a session is already running!");
			}
		}

		private void ServiceHost_SessionStopRequested(SessionStopEventArgs args)
		{
			if (SessionIsRunning)
			{
				if (Session.SessionId == args.SessionId)
				{
					StopSession();
				}
				else
				{
					logger.Warn("Received session stop request with wrong session ID!");
				}
			}
			else
			{
				logger.Warn("Received session stop request, even though no session is currently running!");
			}
		}

		private string AppendDivider(string message)
		{
			var dashesLeft = new String('-', 48 - message.Length / 2 - message.Length % 2);
			var dashesRight = new String('-', 48 - message.Length / 2);

			return $"### {dashesLeft} {message} {dashesRight} ###";
		}
	}
}
