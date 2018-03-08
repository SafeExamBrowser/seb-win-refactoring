/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Contracts.Communication
{
	/// <summary>
	/// Defines the functionality of the communication host for the runtime application component.
	/// </summary>
	public interface IRuntimeHost : ICommunicationHost
	{
		/// <summary>
		/// The startup token used for initial authentication.
		/// </summary>
		Guid StartupToken { set; }

		/// <summary>
		/// Event fired when the client disconnected from the runtime.
		/// </summary>
		event CommunicationEventHandler ClientDisconnected;

		/// <summary>
		/// Event fired once the client has signaled that it is ready to operate.
		/// </summary>
		event CommunicationEventHandler ClientReady;

		/// <summary>
		/// Event fired when the client detected a reconfiguration request.
		/// </summary>
		event CommunicationEventHandler ReconfigurationRequested;

		/// <summary>
		/// Event fired when the client requests to shut down the application.
		/// </summary>
		event CommunicationEventHandler ShutdownRequested;
	}
}
