/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Contracts.Communication.Messages
{
	[Serializable]
	public enum SimpleMessagePurport
	{
		/// <summary>
		/// Requests an interlocutor to submit data for authentication.
		/// </summary>
		Authenticate = 1,

		/// <summary>
		/// Sent from the client to the runtime to indicate that the client is ready to operate.
		/// </summary>
		ClientIsReady,

		/// <summary>
		/// Sent from the client to the runtime to ask for the client configuration.
		/// </summary>
		ConfigurationNeeded,

		/// <summary>
		/// Sent from the client to the runtime to request shutting down the application.
		/// </summary>
		RequestShutdown,

		/// <summary>
		/// Sent form the runtime to the client to command the latter to shut itself down.
		/// </summary>
		Shutdown
	}
}
