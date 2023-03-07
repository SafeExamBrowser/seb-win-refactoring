/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Configuration.Contracts;

namespace SafeExamBrowser.Communication.Contracts.Proxies
{
	/// <summary>
	/// Defines the functionality for a proxy to the communication host of the service application component.
	/// </summary>
	public interface IServiceProxy : ICommunicationProxy
	{
		/// <summary>
		/// Instructs the service to start a system configuration update.
		/// </summary>
		CommunicationResult RunSystemConfigurationUpdate();

		/// <summary>
		/// Instructs the service to start a new session according to the given configuration.
		/// </summary>
		CommunicationResult StartSession(ServiceConfiguration configuration);

		/// <summary>
		/// Instructs the service to stop the specified session.
		/// </summary>
		CommunicationResult StopSession(Guid sessionId);
	}
}
