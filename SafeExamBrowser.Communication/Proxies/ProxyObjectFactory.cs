/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ServiceModel;
using SafeExamBrowser.Communication.Contracts.Proxies;

namespace SafeExamBrowser.Communication.Proxies
{
	/// <summary>
	/// Default implementation of the <see cref="IProxyObjectFactory"/> utilizing WCF (<see cref="ChannelFactory"/>).
	/// </summary>
	public class ProxyObjectFactory : IProxyObjectFactory
	{
		public IProxyObject CreateObject(string address)
		{
			var endpoint = new EndpointAddress(address);
			var channel = ChannelFactory<IProxyObject>.CreateChannel(new NetNamedPipeBinding(NetNamedPipeSecurityMode.Transport), endpoint);

			return channel;
		}
	}
}
