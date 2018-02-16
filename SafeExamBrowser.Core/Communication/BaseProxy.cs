/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ServiceModel;
using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.Communication.Messages;
using SafeExamBrowser.Contracts.Communication.Responses;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Core.Communication
{
	public abstract class BaseProxy : ICommunicationProxy
	{
		private string address;
		private ICommunication channel;
		private Guid? communicationToken;

		protected ILogger Logger { get; private set; }

		public BaseProxy(string address, ILogger logger)
		{
			this.address = address;
			this.Logger = logger;
		}

		public virtual bool Connect(Guid? token = null)
		{
			var endpoint = new EndpointAddress(address);

			channel = ChannelFactory<ICommunication>.CreateChannel(new NetNamedPipeBinding(NetNamedPipeSecurityMode.Transport), endpoint);
			(channel as ICommunicationObject).Closed += BaseProxy_Closed;
			(channel as ICommunicationObject).Closing += BaseProxy_Closing;
			(channel as ICommunicationObject).Faulted += BaseProxy_Faulted;
			(channel as ICommunicationObject).Opened += BaseProxy_Opened;
			(channel as ICommunicationObject).Opening += BaseProxy_Opening;

			Logger.Debug($"Trying to connect to endpoint {address}{(token.HasValue ? $" with authentication token '{token}'" : string.Empty)}...");

			var response = channel.Connect(token);

			communicationToken = response.CommunicationToken;
			Logger.Debug($"Connection was {(response.ConnectionEstablished ? "established" : "refused")}.");

			return response.ConnectionEstablished;
		}

		public virtual bool Disconnect()
		{
			FailIfNotConnected(nameof(Disconnect));

			var message = new DisconnectionMessage { CommunicationToken = communicationToken.Value };
			var response = channel.Disconnect(message);

			Logger.Debug($"{(response.ConnectionTerminated ? "Disconnected" : "Failed to disconnect")} from {address}.");

			return response.ConnectionTerminated;
		}

		protected Response Send(Message message)
		{
			FailIfNotConnected(nameof(Send));

			message.CommunicationToken = communicationToken.Value;

			var response = channel.Send(message);

			Logger.Debug($"Sent message '{ToString(message)}', got response '{ToString(response)}'.");

			return response;
		}

		protected Response Send(SimpleMessagePurport purport)
		{
			return Send(new SimpleMessage(purport));
		}

		private void BaseProxy_Closed(object sender, EventArgs e)
		{
			Logger.Debug("Communication channel has been closed.");
		}

		private void BaseProxy_Closing(object sender, EventArgs e)
		{
			Logger.Debug("Communication channel is closing...");
		}

		private void BaseProxy_Faulted(object sender, EventArgs e)
		{
			Logger.Debug("Communication channel has faulted!");
		}

		private void BaseProxy_Opened(object sender, EventArgs e)
		{
			Logger.Debug("Communication channel has been opened.");
		}

		private void BaseProxy_Opening(object sender, EventArgs e)
		{
			Logger.Debug("Communication channel is opening...");
		}

		private void FailIfNotConnected(string operationName)
		{
			if (!communicationToken.HasValue)
			{
				throw new InvalidOperationException($"Cannot perform '{operationName}' before being connected to endpoint!");
			}

			if (channel == null || (channel as ICommunicationObject).State != CommunicationState.Opened)
			{
				throw new CommunicationException($"Tried to perform {operationName}, but channel was {GetChannelState()}!");
			}
		}

		private string GetChannelState()
		{
			return channel == null ? "null" : $"in state '{(channel as ICommunicationObject).State}'";
		}

		private string ToString(Message message)
		{
			return message != null ? message.ToString() : "<null>";
		}

		private string ToString(Response response)
		{
			return response != null ? response.ToString() : "<null>";
		}
	}
}
