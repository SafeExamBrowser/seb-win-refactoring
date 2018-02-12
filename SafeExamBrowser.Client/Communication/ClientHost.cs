/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.Communication.Messages;
using SafeExamBrowser.Contracts.Communication.Responses;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Core.Communication;

namespace SafeExamBrowser.Client.Communication
{
	internal class ClientHost : BaseHost, IClientHost
	{
		public ClientHost(string address, ILogger logger) : base(address, logger)
		{
		}

		protected override IConnectResponse OnConnect(Guid? token)
		{
			// TODO
			throw new NotImplementedException();
		}

		protected override void OnDisconnect(IMessage message)
		{
			// TODO
			throw new NotImplementedException();
		}

		protected override IResponse OnReceive(IMessage message)
		{
			// TODO
			throw new NotImplementedException();
		}
	}
}
