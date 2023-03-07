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
	/// The reply to a <see cref="PasswordRequestMessage"/>.
	/// </summary>
	[Serializable]
	public class PasswordReplyMessage : Message
	{
		/// <summary>
		/// The password entered by the user, or <c>null</c> if the user interaction was unsuccessful.
		/// </summary>
		public string Password { get; private set; }

		/// <summary>
		/// The unique identifier for the password request.
		/// </summary>
		public Guid RequestId { get; private set; }

		/// <summary>
		/// Determines whether the user interaction was successful or not.
		/// </summary>
		public bool Success { get; private set; }

		public PasswordReplyMessage(Guid requestId, bool success, string password = null)
		{
			Password = password;
			RequestId = requestId;
			Success = success;
		}
	}
}
