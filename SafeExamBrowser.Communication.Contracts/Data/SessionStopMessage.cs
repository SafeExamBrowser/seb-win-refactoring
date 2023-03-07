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
	/// This message is transmitted to the service to request the termination of a currently running session.
	/// </summary>
	[Serializable]
	public class SessionStopMessage : Message
	{
		public Guid SessionId { get; }

		public SessionStopMessage(Guid sessionId)
		{
			SessionId = sessionId;
		}
	}
}
