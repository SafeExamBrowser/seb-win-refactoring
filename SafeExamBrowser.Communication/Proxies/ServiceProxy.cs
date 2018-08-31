/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Communication.Proxies
{
	/// <summary>
	/// Default implementation of the <see cref="IServiceProxy"/>, to be used for communication with the service application component.
	/// </summary>
	public class ServiceProxy : BaseProxy, IServiceProxy
	{
		public bool Ignore { private get; set; }

		public ServiceProxy(string address, IProxyObjectFactory factory, ILogger logger) : base(address, factory, logger)
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

		public CommunicationResult StartSession(Guid sessionId, Settings settings)
		{
			if (IgnoreOperation(nameof(StartSession)))
			{
				return new CommunicationResult(true);
			}

			// TODO: Implement service communication
			// Send(new StartSessionMessage { Id = sessionId, Settings = settings });

			throw new NotImplementedException();
		}

		public CommunicationResult StopSession(Guid sessionId)
		{
			if (IgnoreOperation(nameof(StopSession)))
			{
				return new CommunicationResult(true);
			}

			// TODO: Implement service communication
			// Send(new StopSessionMessage { SessionId = sessionId });

			throw new NotImplementedException();
		}

		private bool IgnoreOperation(string operationName)
		{
			if (Ignore)
			{
				Logger.Debug($"Skipping {operationName} because the ignore flag is set.");
			}

			return Ignore;
		}
	}
}
