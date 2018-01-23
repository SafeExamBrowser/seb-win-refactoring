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
	public class CommunicationHostProxy : ICommunicationHost
	{
		private string address;
		private ILogger logger;
		private ICommunicationHost channel;

		public CommunicationHostProxy(ILogger logger, string address)
		{
			this.address = address;
			this.logger = logger;
		}

		public IConnectResponse Connect(Guid? token = null)
		{
			var endpoint = new EndpointAddress(address);

			channel = ChannelFactory<ICommunicationHost>.CreateChannel(new NetNamedPipeBinding(NetNamedPipeSecurityMode.Transport), endpoint);
			(channel as ICommunicationObject).Closed += CommunicationHostProxy_Closed;
			(channel as ICommunicationObject).Closing += CommunicationHostProxy_Closing;
			(channel as ICommunicationObject).Faulted += CommunicationHostProxy_Faulted;

			var response = channel.Connect(token);

			logger.Debug($"Tried to connect to {address}, connection was {(response.ConnectionEstablished ? "established" : "refused")}.");

			return response;
		}

		public void Disconnect(IMessage message)
		{
			if (ChannelIsReady())
			{
				channel.Disconnect(message);
				logger.Debug($"Disconnected from {address}, transmitting {ToString(message)}.");
			}

			throw new CommunicationException($"Tried to disconnect from host, but channel was {GetChannelState()}!");
		}

		public IResponse Send(IMessage message)
		{
			if (ChannelIsReady())
			{
				var response = channel.Send(message);

				logger.Debug($"Sent {ToString(message)}, got {ToString(response)}.");

				return response;
			}

			throw new CommunicationException($"Tried to send {ToString(message)}, but channel was {GetChannelState()}!");
		}

		private bool ChannelIsReady()
		{
			return channel != null && (channel as ICommunicationObject).State == CommunicationState.Opened;
		}

		private void CommunicationHostProxy_Closed(object sender, EventArgs e)
		{
			logger.Debug("Communication channel has been closed.");
		}

		private void CommunicationHostProxy_Closing(object sender, EventArgs e)
		{
			logger.Debug("Communication channel is closing.");
		}

		private void CommunicationHostProxy_Faulted(object sender, EventArgs e)
		{
			logger.Error("Communication channel has faulted!");
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
