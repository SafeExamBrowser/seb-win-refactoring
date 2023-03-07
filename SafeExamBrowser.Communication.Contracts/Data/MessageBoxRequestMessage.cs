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
	/// This message is transmitted to the client to request a message box input by the user.
	/// </summary>
	[Serializable]
	public class MessageBoxRequestMessage : Message
	{
		/// <summary>
		/// The action to be displayed.
		/// </summary>
		public int Action { get; private set; }

		/// <summary>
		/// The icon to be displayed.
		/// </summary>
		public int Icon { get; private set; }

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

		public MessageBoxRequestMessage(int action, int icon, string message, Guid requestId, string title)
		{
			Action = action;
			Icon = icon;
			Message = message;
			RequestId = requestId;
			Title = title;
		}
	}
}
