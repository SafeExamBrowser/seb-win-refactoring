/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Configuration.Contracts;

namespace SafeExamBrowser.Communication.Contracts.Data
{
	/// <summary>
	/// The response to be used to reply to a configuration request (see <see cref="SimpleMessagePurport.ConfigurationNeeded"/>).
	/// </summary>
	[Serializable]
	public class ConfigurationResponse : Response
	{
		/// <summary>
		/// The configuration to be used by the client.
		/// </summary>
		public ClientConfiguration Configuration { get; set; }
	}
}
