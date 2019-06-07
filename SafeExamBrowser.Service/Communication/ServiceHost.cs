/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Communication.Hosts;
using SafeExamBrowser.Contracts.Communication.Data;
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Service.Communication
{
	internal class ServiceHost : BaseHost, IServiceHost
	{
		internal ServiceHost(string address, IHostObjectFactory factory, ILogger logger, int timeout_ms) : base(address, factory, logger, timeout_ms)
		{
		}

		protected override bool OnConnect(Guid? token)
		{
			throw new NotImplementedException();
		}

		protected override void OnDisconnect()
		{
			throw new NotImplementedException();
		}

		protected override Response OnReceive(Message message)
		{
			throw new NotImplementedException();
		}

		protected override Response OnReceive(SimpleMessagePurport message)
		{
			throw new NotImplementedException();
		}
	}
}
