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
	/// <summary>
	/// Defines the functionality for a proxy to the communication host of the runtime application component.
	/// </summary>
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

		/// <summary>
		/// Requests the runtime to reconfigure the application with the configuration from the given location. Returns <c>true</c> if
		/// the runtime accepted the request, otherwise <c>false</c>.
		/// </summary>
		/// <exception cref="System.ServiceModel.*">If the communication failed.</exception>
		bool RequestReconfiguration(string url);
	}
}
