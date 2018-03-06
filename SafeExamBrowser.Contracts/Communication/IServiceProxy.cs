/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Configuration.Settings;

namespace SafeExamBrowser.Contracts.Communication
{
	/// <summary>
	/// Defines the functionality for a proxy to the communication host of the service application component.
	/// </summary>
	public interface IServiceProxy : ICommunicationProxy
	{
		/// <summary>
		/// Instructs the proxy to ignore all operations or simulate a connection, where applicable. To be set e.g. when the service
		/// policy is optional and the service is not available.
		/// </summary>
		bool Ignore { set; }

		/// <summary>
		/// Instructs the service to start a new session according to the given parameters.
		/// </summary>
		/// <exception cref="System.ServiceModel.*">If the communication failed.</exception>
		void StartSession(Guid sessionId, Settings settings);

		/// <summary>
		/// Instructs the service to stop the specified session.
		/// </summary>
		/// <exception cref="System.ServiceModel.*">If the communication failed.</exception>
		void StopSession(Guid sessionId);
	}
}
