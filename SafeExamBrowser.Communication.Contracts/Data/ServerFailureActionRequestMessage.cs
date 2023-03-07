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
	/// This message is transmitted to the client to request a server failure action selection by the user.
	/// </summary>
	[Serializable]
	public class ServerFailureActionRequestMessage : Message
	{
		/// <summary>
		/// The server failure message, if available.
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// Indicates whether the fallback option should be shown to the user.
		/// </summary>
		public bool ShowFallback { get; set; }

		/// <summary>
		/// The unique identifier for the server failure action selection request.
		/// </summary>
		public Guid RequestId { get; }

		public ServerFailureActionRequestMessage(string message, bool showFallback, Guid requestId)
		{
			Message = message;
			ShowFallback = showFallback;
			RequestId = requestId;
		}
	}
}
