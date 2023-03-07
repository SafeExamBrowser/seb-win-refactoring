/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ServiceModel;
using SafeExamBrowser.Communication.Contracts;
using SafeExamBrowser.Communication.Contracts.Hosts;

namespace SafeExamBrowser.Communication.Hosts
{
	/// <summary>
	/// Default implementation of the <see cref="IHostObjectFactory"/> utilizing WCF (<see cref="ServiceHost"/>).
	/// </summary>
	public class HostObjectFactory : IHostObjectFactory
	{
		public IHostObject CreateObject(string address, ICommunication communicationObject)
		{
			var host = new Host(communicationObject);

			host.AddServiceEndpoint(typeof(ICommunication), new NetNamedPipeBinding(NetNamedPipeSecurityMode.Transport), address);

			return host;
		}

		private class Host : ServiceHost, IHostObject
		{
			internal Host(object singletonInstance, params Uri[] baseAddresses) : base(singletonInstance, baseAddresses)
			{
			}
		}
	}
}
