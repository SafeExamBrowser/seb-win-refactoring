/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Communication.Contracts;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Communication.Proxies
{
	/// <summary>
	/// Default implementation of the <see cref="IProxyFactory"/>, creating instances of the default proxy implementations.
	/// </summary>
	public class ProxyFactory : IProxyFactory
	{
		private IProxyObjectFactory factory;
		private IModuleLogger logger;

		public ProxyFactory(IProxyObjectFactory factory, IModuleLogger logger)
		{
			this.factory = factory;
			this.logger = logger;
		}

		public IClientProxy CreateClientProxy(string address, Interlocutor owner)
		{
			return new ClientProxy(address, factory, logger.CloneFor(nameof(ClientProxy)), owner);
		}
	}
}
