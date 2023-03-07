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
	/// A factory to create communication proxies during application runtime.
	/// </summary>
	public interface IProxyFactory
	{
		/// <summary>
		/// Creates a new <see cref="IClientProxy"/> for the given endpoint address and owner.
		/// </summary>
		IClientProxy CreateClientProxy(string address, Interlocutor owner);
	}
}
