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
	/// This message is transmitted to the client to request a password input by the user.
	/// </summary>
	[Serializable]
	public class PasswordRequestMessage : Message
	{
		/// <summary>
		/// The purpose of the password request.
		/// </summary>
		public PasswordRequestPurpose Purpose { get; private set; }

		/// <summary>
		/// The unique identifier for the password request.
		/// </summary>
		public Guid RequestId { get; private set; }

		public PasswordRequestMessage(PasswordRequestPurpose purpose, Guid requestId)
		{
			Purpose = purpose;
			RequestId = requestId;
		}
	}
}
