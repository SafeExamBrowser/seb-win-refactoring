/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ServiceModel;
using SafeExamBrowser.Contracts.Communication.Data;
using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Core.Communication.Proxies
{
	/// <summary>
	/// Default implementation of the <see cref="IRuntimeProxy"/>, to be used for communication with the runtime application component.
	/// </summary>
	public class RuntimeProxy : BaseProxy, IRuntimeProxy
	{
		public RuntimeProxy(string address, IProxyObjectFactory factory, ILogger logger) : base(address, factory, logger)
		{
		}

		public ClientConfiguration GetConfiguration()
		{
			var response = Send(SimpleMessagePurport.ConfigurationNeeded);

			if (response is ConfigurationResponse configurationResponse)
			{
				return configurationResponse.Configuration;
			}

			throw new CommunicationException($"Could not retrieve client configuration! Received: {ToString(response)}.");
		}

		public void InformClientReady()
		{
			var response = Send(SimpleMessagePurport.ClientIsReady);

			if (!IsAcknowledged(response))
			{
				throw new CommunicationException($"Runtime did not acknowledge that client is ready! Response: {ToString(response)}.");
			}
		}

		public void RequestReconfiguration(string filePath)
		{
			var response = Send(new ReconfigurationMessage(filePath));

			if (!IsAcknowledged(response))
			{
				throw new CommunicationException($"Runtime did not acknowledge reconfiguration request! Response: {ToString(response)}.");
			}
		}

		public void RequestShutdown()
		{
			var response = Send(SimpleMessagePurport.RequestShutdown);

			if (!IsAcknowledged(response))
			{
				throw new CommunicationException($"Runtime did not acknowledge shutdown request! Response: {ToString(response)}.");
			}
		}
	}
}
