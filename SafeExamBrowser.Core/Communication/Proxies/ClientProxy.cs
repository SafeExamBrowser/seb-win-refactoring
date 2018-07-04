/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ServiceModel;
using SafeExamBrowser.Contracts.Communication.Data;
using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Core.Communication.Proxies
{
	/// <summary>
	/// Default implementation of the <see cref="IClientProxy"/>, to be used for communication with the client application component.
	/// </summary>
	public class ClientProxy : BaseProxy, IClientProxy
	{
		public ClientProxy(string address, IProxyObjectFactory factory, ILogger logger) : base(address, factory, logger)
		{
		}

		public void InformReconfigurationDenied(string filePath)
		{
			var response = Send(new ReconfigurationDeniedMessage(filePath));

			if (!IsAcknowledged(response))
			{
				throw new CommunicationException($"Client did not acknowledge shutdown request! Received: {ToString(response)}.");
			}
		}

		public void InitiateShutdown()
		{
			var response = Send(SimpleMessagePurport.Shutdown);

			if (!IsAcknowledged(response))
			{
				throw new CommunicationException($"Client did not acknowledge shutdown request! Received: {ToString(response)}.");
			}
		}

		public AuthenticationResponse RequestAuthentication()
		{
			var response = Send(SimpleMessagePurport.Authenticate);

			if (response is AuthenticationResponse authenticationResponse)
			{
				return authenticationResponse;
			}

			throw new CommunicationException($"Did not receive authentication response! Received: {ToString(response)}.");
		}

		public void RequestPassword(PasswordRequestPurpose purpose, Guid requestId)
		{
			var response = Send(new PasswordRequestMessage(purpose, requestId));

			if (!IsAcknowledged(response))
			{
				throw new CommunicationException($"Client did not acknowledge shutdown request! Received: {ToString(response)}.");
			}
		}
	}
}
