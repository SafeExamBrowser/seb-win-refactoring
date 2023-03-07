/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Communication.Contracts.Data;

namespace SafeExamBrowser.Communication.Contracts.Events
{
	/// <summary>
	/// The event arguments used for the password request event fired by the <see cref="Hosts.IClientHost"/>.
	/// </summary>
	public class PasswordRequestEventArgs : CommunicationEventArgs
	{
		/// <summary>
		/// The purpose for which a password is requested.
		/// </summary>
		public PasswordRequestPurpose Purpose { get; set; }

		/// <summary>
		/// Identifies the password request.
		/// </summary>
		public Guid RequestId { get; set; }
	}
}
