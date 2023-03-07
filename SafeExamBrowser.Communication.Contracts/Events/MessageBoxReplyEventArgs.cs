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
	/// The event arguments used for the message box reply event fired by the <see cref="Hosts.IRuntimeHost"/>.
	/// </summary>
	public class MessageBoxReplyEventArgs : CommunicationEventArgs
	{
		/// <summary>
		/// Identifies the message box request.
		/// </summary>
		public Guid RequestId { get; set; }

		/// <summary>
		/// The result of the interaction.
		/// </summary>
		public int Result { get; set; }
	}
}
