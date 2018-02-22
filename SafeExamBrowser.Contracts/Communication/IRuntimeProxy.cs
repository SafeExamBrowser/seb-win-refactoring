/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Configuration;

namespace SafeExamBrowser.Contracts.Communication
{
	public interface IRuntimeProxy : ICommunicationProxy
	{
		/// <summary>
		/// Retrieves the application configuration from the runtime.
		/// </summary>
		/// <exception cref="System.ServiceModel.*">If the communication failed.</exception>
		ClientConfiguration GetConfiguration();

		/// <summary>
		/// Informs the runtime that the client is ready.
		/// </summary>
		/// <exception cref="System.ServiceModel.*">If the communication failed.</exception>
		void InformClientReady();

		/// <summary>
		/// Requests the runtime to shut down the application.
		/// </summary>
		/// <exception cref="System.ServiceModel.*">If the communication failed.</exception>
		void RequestShutdown();
	}
}
