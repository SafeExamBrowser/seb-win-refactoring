/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Communication.Contracts.Hosts
{
	/// <summary>
	/// A factory to create host objects for communication hosts.
	/// </summary>
	public interface IHostObjectFactory
	{
		/// <summary>
		/// Utilizes the given communication object to create a host object (see <see cref="IHostObject"/>) for the specified endpoint address.
		/// </summary>
		IHostObject CreateObject(string address, ICommunication communicationObject);
	}
}
