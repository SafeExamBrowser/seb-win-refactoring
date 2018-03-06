/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ServiceModel;
using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.Communication.Messages;
using SafeExamBrowser.Contracts.Communication.Responses;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Core.Communication
{
	/// <summary>
	/// Default implementation of the <see cref="IClientProxy"/>, to be used for communication with the client application component.
	/// </summary>
	public class ClientProxy : BaseProxy, IClientProxy
	{
		public ClientProxy(string address, ILogger logger) : base(address, logger)
		{
		}

		public void InitiateShutdown()
		{
			var response = Send(SimpleMessagePurport.Shutdown);

			if (!IsAcknowledged(response))
			{
				throw new CommunicationException($"Runtime did not acknowledge shutdown request! Received: {ToString(response)}.");
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
	}
}
