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
	/// Defines the common functionality for all communication proxies. A proxy is needed to be able to perform inter-process communication
	/// with the <see cref="ICommunicationHost"/> of another application component.
	/// </summary>
	public interface ICommunicationProxy
	{
		/// <summary>
		/// Fired when the connection to the host was lost, e.g. if a ping request failed or a communication fault occurred.
		/// </summary>
		event CommunicationEventHandler ConnectionLost;

		/// <summary>
		/// Tries to establish a connection. Returns <c>true</c> if the connection has been accepted, otherwise <c>false</c>. If a
		/// connection was successfully established and the auto-ping flag is set, the connection status will be periodically checked.
		/// </summary>
		/// <exception cref="System.ServiceModel.*">If the communication failed.</exception>
		bool Connect(Guid? token = null, bool autoPing = true);

		/// <summary>
		/// Terminates an open connection. Returns <c>true</c> if the disconnection has been acknowledged, otherwise <c>false</c>.
		/// </summary>
		/// <exception cref="InvalidOperationException">If no connection has been established.</exception>
		/// <exception cref="System.ServiceModel.*">If the communication failed.</exception>
		bool Disconnect();
	}
}
