/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Communication.Contracts.Proxies
{
	/// <summary>
	/// A factory to create proxy objects for communication proxies.
	/// </summary>
	public interface IProxyObjectFactory
	{
		/// <summary>
		/// Creates a proxy object (see <see cref="IProxyObject"/>) for the specified endpoint address.
		/// </summary>
		IProxyObject CreateObject(string address);
	}
}
