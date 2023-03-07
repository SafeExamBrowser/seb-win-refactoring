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
	/// The response transmitted to a <see cref="DisconnectionMessage"/>
	/// </summary>
	[Serializable]
	public class DisconnectionResponse : Response
	{
		/// <summary>
		/// Indicates whether the connection has been terminated.
		/// </summary>
		public bool ConnectionTerminated { get; set; }
	}
}
