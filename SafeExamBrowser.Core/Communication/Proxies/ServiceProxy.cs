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

namespace SafeExamBrowser.Core.Communication.Proxies
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
				return true;
			}

			return base.Disconnect();
		}

		public void StartSession(Guid sessionId, Settings settings)
		{
			if (IgnoreOperation(nameof(StartSession)))
			{
				return;
			}

			// TODO: Implement service communication
			// Send(new StartSessionMessage { Id = sessionId, Settings = settings });
		}

		public void StopSession(Guid sessionId)
		{
			if (IgnoreOperation(nameof(StopSession)))
			{
				return;
			}

			// TODO: Implement service communication
			// Send(new StopSessionMessage { SessionId = sessionId });
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
