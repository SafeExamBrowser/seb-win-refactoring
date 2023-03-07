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
	/// A generic message to send simple purports without data to an interlocutor.
	/// </summary>
	[Serializable]
	public class SimpleMessage : Message
	{
		/// <summary>
		/// The purport of the message.
		/// </summary>
		public SimpleMessagePurport Purport { get; set; }

		public SimpleMessage(SimpleMessagePurport purport)
		{
			Purport = purport;
		}

		public override string ToString()
		{
			return $"{base.ToString()} -> {Purport}";
		}
	}
}
