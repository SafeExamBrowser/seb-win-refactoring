/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Communication.Data;

namespace SafeExamBrowser.Contracts.Communication.Proxies
{
	/// <summary>
	/// Defines the functionality for a proxy to the communication host of the client application component.
	/// </summary>
	public interface IClientProxy : ICommunicationProxy
	{
		/// <summary>
		/// Instructs the client to initiate its shutdown procedure.
		/// </summary>
		/// <exception cref="System.ServiceModel.*">If the communication failed.</exception>
		void InitiateShutdown();

		/// <summary>
		/// Instructs the client to submit its authentication data.
		/// </summary>
		/// <exception cref="System.ServiceModel.*">If the communication failed.</exception>
		AuthenticationResponse RequestAuthentication();
	}
}
