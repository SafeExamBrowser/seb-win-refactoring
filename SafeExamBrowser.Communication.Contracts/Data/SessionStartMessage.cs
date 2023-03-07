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
	/// This message is transmitted to the service to request the initialization of a new session.
	/// </summary>
	[Serializable]
	public class SessionStartMessage : Message
	{
		/// <summary>
		/// The configuration to be used by the service.
		/// </summary>
		public ServiceConfiguration Configuration { get; }

		public SessionStartMessage(ServiceConfiguration configuration)
		{
			Configuration = configuration;
		}
	}
}
