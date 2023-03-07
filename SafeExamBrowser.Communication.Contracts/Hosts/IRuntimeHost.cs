/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Communication.Contracts.Events;

namespace SafeExamBrowser.Communication.Contracts.Hosts
{
	/// <summary>
	/// Defines the functionality of the communication host for the runtime application component.
	/// </summary>
	public interface IRuntimeHost : ICommunicationHost
	{
		/// <summary>
		/// Determines whether another application component may establish a connection with the host.
		/// </summary>
		bool AllowConnection { get; set; }

		/// <summary>
		/// The token used for initial authentication.
		/// </summary>
		Guid? AuthenticationToken { set; }

		/// <summary>
		/// Event fired when the client disconnected from the runtime.
		/// </summary>
		event CommunicationEventHandler ClientDisconnected;

		/// <summary>
		/// Event fired when the client has signaled that it is ready to operate.
		/// </summary>
		event CommunicationEventHandler ClientReady;

		/// <summary>
		/// Event fired when the client requested its configuration data.
		/// </summary>
		event CommunicationEventHandler<ClientConfigurationEventArgs> ClientConfigurationNeeded;

		/// <summary>
		/// Event fired when the client submitted a server exam selection made by the user.
		/// </summary>
		event CommunicationEventHandler<ExamSelectionReplyEventArgs> ExamSelectionReceived;

		/// <summary>
		/// Event fired when the client submitted a message box result chosen by the user.
		/// </summary>
		event CommunicationEventHandler<MessageBoxReplyEventArgs> MessageBoxReplyReceived;

		/// <summary>
		/// Event fired when the client submitted a password entered by the user.
		/// </summary>
		event CommunicationEventHandler<PasswordReplyEventArgs> PasswordReceived;

		/// <summary>
		/// Event fired when the client requested a reconfiguration of the application.
		/// </summary>
		event CommunicationEventHandler<ReconfigurationEventArgs> ReconfigurationRequested;

		/// <summary>
		/// Event fired when the client submitted a server failure action chosen by the user.
		/// </summary>
		event CommunicationEventHandler<ServerFailureActionReplyEventArgs> ServerFailureActionReceived;

		/// <summary>
		/// Event fired when the client requests to shut down the application.
		/// </summary>
		event CommunicationEventHandler ShutdownRequested;
	}
}
