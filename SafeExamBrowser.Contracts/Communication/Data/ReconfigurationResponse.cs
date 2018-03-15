/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Contracts.Communication.Data
{
	/// <summary>
	/// The response to a <see cref="Messages.ReconfigurationMessage"/>.
	/// </summary>
	[Serializable]
	public class ReconfigurationResponse : Response
	{
		/// <summary>
		/// Indicates whether the reconfiguration request has been accepted.
		/// </summary>
		public bool Accepted { get; set; }
	}
}
