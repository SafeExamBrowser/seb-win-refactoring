/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Communication.Contracts.Events
{
	/// <summary>
	/// The event arguments used for the password input event fired by the <see cref="Hosts.IRuntimeHost"/>.
	/// </summary>
	public class PasswordReplyEventArgs : CommunicationEventArgs
	{
		/// <summary>
		/// The password entered by the user, or <c>null</c> if not available.
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// Identifies the password request.
		/// </summary>
		public Guid RequestId { get; set; }

		/// <summary>
		/// Indicates whether the password has been successfully entered by the user.
		/// </summary>
		public bool Success { get; set; }
	}
}
