/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ServiceModel;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Configuration.Contracts;
using ServiceConfiguration = SafeExamBrowser.Configuration.Contracts.ServiceConfiguration;

namespace SafeExamBrowser.Communication.Contracts
{
	/// <summary>
	/// Defines the API for all communication between the three application components (runtime, service and client).
	/// </summary>
	[ServiceContract(SessionMode = SessionMode.Required)]
	[ServiceKnownType(typeof(AuthenticationResponse))]
	[ServiceKnownType(typeof(ClientConfiguration))]
	[ServiceKnownType(typeof(ConfigurationResponse))]
	[ServiceKnownType(typeof(ExamSelectionReplyMessage))]
	[ServiceKnownType(typeof(ExamSelectionRequestMessage))]
	[ServiceKnownType(typeof(MessageBoxReplyMessage))]
	[ServiceKnownType(typeof(MessageBoxRequestMessage))]
	[ServiceKnownType(typeof(PasswordReplyMessage))]
	[ServiceKnownType(typeof(PasswordRequestMessage))]
	[ServiceKnownType(typeof(ReconfigurationMessage))]
	[ServiceKnownType(typeof(ReconfigurationDeniedMessage))]
	[ServiceKnownType(typeof(ServerFailureActionReplyMessage))]
	[ServiceKnownType(typeof(ServerFailureActionRequestMessage))]
	[ServiceKnownType(typeof(ServiceConfiguration))]
	[ServiceKnownType(typeof(SessionStartMessage))]
	[ServiceKnownType(typeof(SessionStopMessage))]
	[ServiceKnownType(typeof(SimpleMessage))]
	[ServiceKnownType(typeof(SimpleResponse))]
	public interface ICommunication
	{
		/// <summary>
		/// Initiates a connection and must thus be called before any other opertion. Where applicable, an authentication token should be
		/// specified. Returns a response indicating whether the connection request was successful or not.
		/// </summary>
		[OperationContract(IsInitiating = true)]
		ConnectionResponse Connect(Guid? token = null);

		/// <summary>
		/// Closes a connection. Returns a response indicating whether the disconnection request was successful or not.
		/// </summary>
		[OperationContract(IsInitiating = false, IsTerminating = true)]
		DisconnectionResponse Disconnect(DisconnectionMessage message);

		/// <summary>
		/// Sends a message, optionally returning a response. If no response is expected, <c>null</c> will be returned.
		/// </summary>
		[OperationContract(IsInitiating = false)]
		Response Send(Message message);
	}
}
