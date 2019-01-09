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
	/// This message is transmitted to the client to request a message box input by the user.
	/// </summary>
	[Serializable]
	public class MessageBoxRequestMessage : Message
	{
		/// <summary>
		/// The action to be displayed.
		/// </summary>
		public MessageBoxAction Action { get; private set; }

		/// <summary>
		/// The icon to be displayed.
		/// </summary>
		public MessageBoxIcon Icon { get; private set; }

		/// <summary>
		/// The message to be displayed.
		/// </summary>
		public string Message { get; private set; }

		/// <summary>
		/// Identifies the message box request.
		/// </summary>
		public Guid RequestId { get; private set; }

		/// <summary>
		/// The title to be displayed.
		/// </summary>
		public string Title { get; private set; }

		public MessageBoxRequestMessage(MessageBoxAction action, MessageBoxIcon icon, string message, Guid requestId, string title)
		{
			Action = action;
			Icon = icon;
			Message = message;
			RequestId = requestId;
			Title = title;
		}
	}
}
