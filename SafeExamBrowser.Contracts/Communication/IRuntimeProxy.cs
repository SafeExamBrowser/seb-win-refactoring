/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Configuration;

namespace SafeExamBrowser.Contracts.Communication
{
	public interface IRuntimeProxy
	{
		/// <summary>
		/// Tries to establish a connection with the runtime host, utilizing the specified authentication token.
		/// </summary>
		bool Connect(Guid token);

		/// <summary>
		/// Disconnects from the runtime host.
		/// </summary>
		void Disconnect();

		/// <summary>
		/// Retrieves the application configuration from the runtime host.
		/// </summary>
		IClientConfiguration GetConfiguration();
	}
}
