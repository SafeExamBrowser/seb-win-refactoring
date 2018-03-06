/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Configuration;

namespace SafeExamBrowser.Contracts.Communication.Responses
{
	/// <summary>
	/// The response to be used to reply to a configuration request (see <see cref="Messages.SimpleMessagePurport.ConfigurationNeeded"/>).
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
