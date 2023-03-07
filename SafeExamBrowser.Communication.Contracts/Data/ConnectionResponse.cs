/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Communication.Contracts.Data
{
	/// <summary>
	/// The response to a connection request (see <see cref="ICommunication.Connect(Guid?)"/>).
	/// </summary>
	[Serializable]
	public class ConnectionResponse : Response
	{
		/// <summary>
		/// The communication token needed for authentication. Is <c>null</c> if a connection was refused.
		/// </summary>
		public Guid? CommunicationToken { get; set; }

		/// <summary>
		/// Indicates whether the connection request has been accepted.
		/// </summary>
		public bool ConnectionEstablished { get; set; }
	}
}
