/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Communication.Events;

namespace SafeExamBrowser.Contracts.Communication.Hosts
{
	/// <summary>
	/// Defines the functionality of the communication host for the service application component.
	/// </summary>
	public interface IServiceHost : ICommunicationHost
	{
		/// <summary>
		/// Determines whether another application component may establish a connection with the host.
		/// </summary>
		bool AllowConnection { get; set; }

		/// <summary>
		/// Event fired when the runtime requested to start a new session.
		/// </summary>
		event CommunicationEventHandler<SessionStartEventArgs> SessionStartRequested;

		/// <summary>
		/// Event fired when the runtime requested to stop a running session.
		/// </summary>
		event CommunicationEventHandler<SessionStopEventArgs> SessionStopRequested;
	}
}
