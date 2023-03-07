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
	/// Defines the functionality of the communication host for the client application component.
	/// </summary>
	public interface IClientHost : ICommunicationHost
	{
		/// <summary>
		/// The token used for initial authentication with the runtime.
		/// </summary>
		Guid AuthenticationToken { set; }

		/// <summary>
		/// Indicates whether the runtime has established a connection to this host.
		/// </summary>
		bool IsConnected { get; }

		/// <summary>
		/// Event fired when the runtime requests a server exam selection from the user.
		/// </summary>
		event CommunicationEventHandler<ExamSelectionRequestEventArgs> ExamSelectionRequested;

		/// <summary>
		/// Event fired when the runtime requests a message box input from the user.
		/// </summary>
		event CommunicationEventHandler<MessageBoxRequestEventArgs> MessageBoxRequested;

		/// <summary>
		/// Event fired when the runtime requests a password input from the user.
		/// </summary>
		event CommunicationEventHandler<PasswordRequestEventArgs> PasswordRequested;

		/// <summary>
		/// Event fired when the runtime aborted a reconfiguration.
		/// </summary>
		event CommunicationEventHandler ReconfigurationAborted;

		/// <summary>
		/// Event fired when the runtime denied a reconfiguration request.
		/// </summary>
		event CommunicationEventHandler<ReconfigurationEventArgs> ReconfigurationDenied;

		/// <summary>
		/// Event fired when the runtime disconnected from the client.
		/// </summary>
		event CommunicationEventHandler RuntimeDisconnected;

		/// <summary>
		/// Event fired when the runtime requests a server failure action selection from the user.
		/// </summary>
		event CommunicationEventHandler<ServerFailureActionRequestEventArgs> ServerFailureActionRequested;

		/// <summary>
		/// Event fired when the runtime commands the client to shutdown.
		/// </summary>
		event CommunicationEventHandler Shutdown;
	}
}
