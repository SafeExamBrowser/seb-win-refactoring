/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Contracts.Communication.Responses
{
	public interface IConnectResponse : IResponse
	{
		/// <summary>
		/// The communication token needed for authentication with the host. Is <c>null</c> if a connection was refused.
		/// </summary>
		Guid? CommunicationToken { get; }

		/// <summary>
		/// Indicates whether the host has accepted the connection request.
		/// </summary>
		bool ConnectionEstablished { get; }
	}
}
