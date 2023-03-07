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
	/// A generic response to reply simple purports without data to an interlocutor.
	/// </summary>
	[Serializable]
	public class SimpleResponse : Response
	{
		/// <summary>
		/// The purport of the response.
		/// </summary>
		public SimpleResponsePurport Purport { get; set; }

		public SimpleResponse(SimpleResponsePurport purport)
		{
			Purport = purport;
		}

		public override string ToString()
		{
			return $"{base.ToString()} -> {Purport}";
		}
	}
}
