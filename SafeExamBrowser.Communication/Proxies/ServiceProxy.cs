/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.Communication.Data;
using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Communication.Proxies
{
	/// <summary>
	/// Default implementation of the <see cref="IServiceProxy"/>, to be used for communication with the service application component.
	/// </summary>
	public class ServiceProxy : BaseProxy, IServiceProxy
	{
		public bool Ignore { private get; set; }

		public ServiceProxy(string address, IProxyObjectFactory factory, ILogger logger, Interlocutor owner) : base(address, factory, logger, owner)
		{
		}

		public override bool Connect(Guid? token = null, bool autoPing = true)
		{
			if (IgnoreOperation(nameof(Connect)))
			{
				return false;
			}

			return base.Connect(autoPing: autoPing);
		}

		public override bool Disconnect()
		{
			if (IgnoreOperation(nameof(Disconnect)))
			{
				return false;
			}

			return base.Disconnect();
		}

		public CommunicationResult RunSystemConfigurationUpdate()
		{
			if (IgnoreOperation(nameof(RunSystemConfigurationUpdate)))
			{
				return new CommunicationResult(true);
			}

			try
			{
				var response = Send(SimpleMessagePurport.UpdateSystemConfiguration);
				var success = IsAcknowledged(response);

				if (success)
				{
					Logger.Debug("Service acknowledged system configuration update.");
				}
				else
				{
					Logger.Error($"Service did not acknowledge system configuration update! Received: {ToString(response)}.");
				}

				return new CommunicationResult(success);
			}
			catch (Exception e)
			{
				Logger.Error($"Failed to perform '{nameof(RunSystemConfigurationUpdate)}'", e);

				return new CommunicationResult(false);
			}
		}

		public CommunicationResult StartSession(ServiceConfiguration configuration)
		{
			if (IgnoreOperation(nameof(StartSession)))
			{
				return new CommunicationResult(true);
			}

			try
			{
				var response = Send(new SessionStartMessage(configuration));
				var success = IsAcknowledged(response);

				if (success)
				{
					Logger.Debug("Service acknowledged session start.");
				}
				else
				{
					Logger.Error($"Service did not acknowledge session start! Received: {ToString(response)}.");
				}

				return new CommunicationResult(success);
			}
			catch (Exception e)
			{
				Logger.Error($"Failed to perform '{nameof(StartSession)}'", e);

				return new CommunicationResult(false);
			}
		}

		public CommunicationResult StopSession(Guid sessionId)
		{
			if (IgnoreOperation(nameof(StopSession)))
			{
				return new CommunicationResult(true);
			}

			try
			{
				var response = Send(new SessionStopMessage(sessionId));
				var success = IsAcknowledged(response);

				if (success)
				{
					Logger.Debug("Service acknowledged session stop.");
				}
				else
				{
					Logger.Error($"Service did not acknowledge session stop! Received: {ToString(response)}.");
				}

				return new CommunicationResult(success);
			}
			catch (Exception e)
			{
				Logger.Error($"Failed to perform '{nameof(StopSession)}'", e);

				return new CommunicationResult(false);
			}
		}

		private bool IgnoreOperation(string operationName)
		{
			if (Ignore)
			{
				Logger.Debug($"Skipping '{operationName}' operation because the ignore flag is set.");
			}

			return Ignore;
		}
	}
}
