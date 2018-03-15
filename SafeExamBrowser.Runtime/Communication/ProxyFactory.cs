/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Core.Communication;
using SafeExamBrowser.Core.Logging;

namespace SafeExamBrowser.Runtime.Communication
{
	internal class ProxyFactory : IProxyFactory
	{
		private IProxyObjectFactory factory;
		private ILogger logger;

		public ProxyFactory(IProxyObjectFactory factory, ILogger logger)
		{
			this.factory = factory;
			this.logger = logger;
		}

		public IClientProxy CreateClientProxy(string address)
		{
			return new ClientProxy(address, factory, new ModuleLogger(logger, typeof(ClientProxy)));
		}
	}
}
