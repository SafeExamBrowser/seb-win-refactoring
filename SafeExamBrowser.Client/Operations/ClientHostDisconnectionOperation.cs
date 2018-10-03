/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Threading;
using SafeExamBrowser.Contracts.Communication.Events;
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Core.OperationModel.Events;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Client.Operations
{
	/// <summary>
	/// During application shutdown, it could happen that the client stops its communication host before the runtime had the chance to
	/// disconnect from it. This operation prevents the described race condition by waiting on the runtime to disconnect from the client.
	/// </summary>
	internal class ClientHostDisconnectionOperation : IOperation
	{
		private IClientHost clientHost;
		private ILogger logger;
		private int timeout_ms;

		public event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public event StatusChangedEventHandler StatusChanged { add { } remove { } }

		public ClientHostDisconnectionOperation(IClientHost clientHost, ILogger logger, int timeout_ms)
		{
			this.clientHost = clientHost;
			this.logger = logger;
			this.timeout_ms = timeout_ms;
		}

		public OperationResult Perform()
		{
			return OperationResult.Success;
		}

		public OperationResult Repeat()
		{
			return OperationResult.Success;
		}

		public void Revert()
		{
			var disconnected = false;
			var disconnectedEvent = new AutoResetEvent(false);
			var disconnectedEventHandler = new CommunicationEventHandler(() => disconnectedEvent.Set());

			// TODO: Update status!

			clientHost.RuntimeDisconnected += disconnectedEventHandler;

			if (clientHost.IsConnected)
			{
				logger.Info("Waiting for runtime to disconnect from client communication host...");
				disconnected = disconnectedEvent.WaitOne(timeout_ms);

				if (!disconnected)
				{
					logger.Error($"Runtime failed to disconnect within {timeout_ms / 1000} seconds!");
				}
			}
			else
			{
				logger.Info("The runtime has already disconnected from the client communication host.");
			}

			clientHost.RuntimeDisconnected -= disconnectedEventHandler;
		}
	}
}
