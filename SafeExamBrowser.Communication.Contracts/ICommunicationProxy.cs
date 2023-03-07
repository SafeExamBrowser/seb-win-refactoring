/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Communication.Contracts.Events;

namespace SafeExamBrowser.Communication.Contracts
{
	/// <summary>
	/// Defines the common functionality for all communication proxies. A proxy is needed to be able to perform inter-process communication
	/// with the <see cref="ICommunicationHost"/> of another application component.
	/// </summary>
	public interface ICommunicationProxy
	{
		/// <summary>
		/// Indicates whether a connection to the host has been established.
		/// </summary>
		bool IsConnected { get; }

		/// <summary>
		/// Fired when the connection to the host was lost, e.g. if a ping request failed or a communication fault occurred.
		/// </summary>
		event CommunicationEventHandler ConnectionLost;

		/// <summary>
		/// Tries to establish a connection. Returns <c>true</c> if the connection has been successful, otherwise <c>false</c>. If a
		/// connection was successfully established and the auto-ping flag is set, the connection status will be periodically checked.
		/// </summary>
		bool Connect(Guid? token = null, bool autoPing = true);

		/// <summary>
		/// Terminates an open connection. Returns <c>true</c> if the disconnection has been successful, otherwise <c>false</c>.
		/// </summary>
		bool Disconnect();
	}
}
