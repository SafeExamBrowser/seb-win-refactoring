/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Communication.Contracts.Events;

namespace SafeExamBrowser.Communication.Contracts.Hosts
{
	/// <summary>
	/// Defines the functionality of the communication host for the service application component.
	/// </summary>
	public interface IServiceHost : ICommunicationHost
	{
		/// <summary>
		/// Event fired when the runtime requested to start a new session.
		/// </summary>
		event CommunicationEventHandler<SessionStartEventArgs> SessionStartRequested;

		/// <summary>
		/// Event fired when the runtime requested to stop a running session.
		/// </summary>
		event CommunicationEventHandler<SessionStopEventArgs> SessionStopRequested;

		/// <summary>
		/// Event fired when the runtime requested to update the system configuration.
		/// </summary>
		event CommunicationEventHandler SystemConfigurationUpdateRequested;
	}
}
