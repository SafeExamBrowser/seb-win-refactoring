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
	/// The event arguments used for the server failure action event fired by the <see cref="Hosts.IRuntimeHost"/>.
	/// </summary>
	public class ServerFailureActionReplyEventArgs : CommunicationEventArgs
	{
		/// <summary>
		/// The user chose to abort the operation.
		/// </summary>
		public bool Abort { get; set; }

		/// <summary>
		/// The user chose to perform a fallback.
		/// </summary>
		public bool Fallback { get; set; }

		/// <summary>
		/// The user chose to retry the operation.
		/// </summary>
		public bool Retry { get; set; }

		/// <summary>
		/// Identifies the server failure action request.
		/// </summary>
		public Guid RequestId { get; set; }
	}
}
