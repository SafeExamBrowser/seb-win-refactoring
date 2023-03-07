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
	/// The possible purports of a <see cref="SimpleResponse"/>.
	/// </summary>
	[Serializable]
	public enum SimpleResponsePurport
	{
		/// <summary>
		/// Signals an interlocutor that a message has been understood.
		/// </summary>
		Acknowledged = 1,

		/// <summary>
		/// Signals an interlocutor that it is not authorized to communicate.
		/// </summary>
		Unauthorized,

		/// <summary>
		/// Signals an interlocutor that a message has not been understood.
		/// </summary>
		UnknownMessage
	}
}
