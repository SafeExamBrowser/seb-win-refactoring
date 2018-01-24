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
	public abstract class BaseProxy : ICommunication
	{
		private string address;
		private ICommunication channel;

		protected Guid? CommunicationToken { get; private set; }
		protected ILogger Logger { get; private set; }

		public BaseProxy(ILogger logger, string address)
		{
			this.address = address;
			this.Logger = logger;
		}

		public IConnectResponse Connect(Guid? token = null)
		{
			var endpoint = new EndpointAddress(address);

			channel = ChannelFactory<ICommunication>.CreateChannel(new NetNamedPipeBinding(NetNamedPipeSecurityMode.Transport), endpoint);
			(channel as ICommunicationObject).Closed += CommunicationHostProxy_Closed;
			(channel as ICommunicationObject).Closing += CommunicationHostProxy_Closing;
			(channel as ICommunicationObject).Faulted += CommunicationHostProxy_Faulted;

			var response = channel.Connect(token);

			CommunicationToken = response.CommunicationToken;
			Logger.Debug($"Tried to connect to {address}, connection was {(response.ConnectionEstablished ? "established" : "refused")}.");

			return response;
		}

		public void Disconnect(IMessage message)
		{
			if (ChannelIsReady())
			{
				channel.Disconnect(message);
				Logger.Debug($"Disconnected from {address}, transmitting {ToString(message)}.");
			}

			throw new CommunicationException($"Tried to disconnect from host, but channel was {GetChannelState()}!");
		}

		public IResponse Send(IMessage message)
		{
			if (ChannelIsReady())
			{
				var response = channel.Send(message);

				Logger.Debug($"Sent {ToString(message)}, got {ToString(response)}.");

				return response;
			}

			throw new CommunicationException($"Tried to send {ToString(message)}, but channel was {GetChannelState()}!");
		}

		protected void FailIfNotConnected(string operationName)
		{
			if (!CommunicationToken.HasValue)
			{
				throw new InvalidOperationException($"Cannot perform '{operationName}' before being connected to endpoint!");
			}
		}

		private bool ChannelIsReady()
		{
			return channel != null && (channel as ICommunicationObject).State == CommunicationState.Opened;
		}

		private void CommunicationHostProxy_Closed(object sender, EventArgs e)
		{
			Logger.Debug("Communication channel has been closed.");
		}

		private void CommunicationHostProxy_Closing(object sender, EventArgs e)
		{
			Logger.Debug("Communication channel is closing.");
		}

		private void CommunicationHostProxy_Faulted(object sender, EventArgs e)
		{
			Logger.Error("Communication channel has faulted!");
		}

		private string GetChannelState()
		{
			return channel == null ? "null" : $"in state '{(channel as ICommunicationObject).State}'";
		}

		private string ToString(IMessage message)
		{
			return message != null ? $"message of type '{message.GetType()}'" : "no message";
		}

		private string ToString(IResponse response)
		{
			return response != null ? $"response of type '{response.GetType()}'" : "no response";
		}
	}
}
