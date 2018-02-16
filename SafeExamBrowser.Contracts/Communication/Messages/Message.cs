/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Contracts.Communication.Messages
{
	[Serializable]
	public class Message
	{
		/// <summary>
		/// The communication token needed for authentication.
		/// </summary>
		public Guid CommunicationToken { get; set; }

		public override string ToString()
		{
			return GetType().Name;
		}
	}
}
