/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.UserInterface.MessageBox;

namespace SafeExamBrowser.Contracts.Communication.Data
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
		public MessageBoxResult Result { get; private set; }

		public MessageBoxReplyMessage(Guid requestId, MessageBoxResult result)
		{
			RequestId = requestId;
			Result = result;
		}
	}
}
