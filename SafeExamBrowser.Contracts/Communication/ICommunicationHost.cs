/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ServiceModel;
using SafeExamBrowser.Contracts.Communication.Messages;
using SafeExamBrowser.Contracts.Communication.Responses;

namespace SafeExamBrowser.Contracts.Communication
{
	[ServiceContract(SessionMode = SessionMode.Required)]
	public interface ICommunicationHost
	{
		/// <summary>
		/// Initiates a connection to the host and must thus be called before any other opertion. To authenticate itself to the host, the
		/// client can specify a security token. If the connection request was successful, a new session will be created by the host and
		/// the client will subsequently be allowed to start communicating with the host.
		/// </summary>
		[OperationContract(IsInitiating = true)]
		IConnectResponse Connect(Guid? token = null);

		/// <summary>
		/// Closes the connection to the host and instructs it to terminate the communication session.
		/// </summary>
		[OperationContract(IsInitiating = false, IsTerminating = true)]
		void Disconnect(IMessage message);

		/// <summary>
		/// Sends a message to the host, optionally returning a response. If no response is expected, <c>null</c> will be returned.
		/// </summary>
		[OperationContract(IsInitiating = false)]
		IResponse Send(IMessage message);
	}
}
