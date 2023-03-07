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
	/// The reply to a <see cref="MessageBoxRequestMessage"/>.
	/// </summary>
	[Serializable]
	public class MessageBoxReplyMessage : Message
	{
		/// <summary>
		/// Identifies the message box request.
		/// </summary>
		public Guid RequestId { get; private set; }

		/// <summary>
		/// The result of the interaction.
		/// </summary>
		public int Result { get; private set; }

		public MessageBoxReplyMessage(Guid requestId, int result)
		{
			RequestId = requestId;
			Result = result;
		}
	}
}
