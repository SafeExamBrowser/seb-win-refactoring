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
	/// This is the last message transmitted from a component to its interlocutor in order to terminate a communication session.
	/// </summary>
	[Serializable]
	public class DisconnectionMessage : Message
	{
		/// <summary>
		/// Identifies the component sending the message.
		/// </summary>
		public Interlocutor Interlocutor { get; set; }
	}
}
