/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Communication.Data;
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Core.Communication.Hosts;

namespace SafeExamBrowser.Core.UnitTests.Communication.Hosts
{
	internal class BaseHostImpl : BaseHost
	{
		public Func<Guid?, bool> OnConnectStub { get; set; }
		public Action OnDisconnectStub { get; set; }
		public Func<Message, Response> OnReceiveStub { get; set; }
		public Func<SimpleMessagePurport, Response> OnReceiveSimpleMessageStub { get; set; }

		public BaseHostImpl(string address, IHostObjectFactory factory, ILogger logger) : base(address, factory, logger)
		{
		}

		public Guid? GetCommunicationToken()
		{
			return CommunicationToken;
		}

		protected override bool OnConnect(Guid? token)
		{
			return OnConnectStub?.Invoke(token) == true;
		}

		protected override void OnDisconnect()
		{
			OnDisconnectStub?.Invoke();
		}

		protected override Response OnReceive(Message message)
		{
			return OnReceiveStub?.Invoke(message);
		}

		protected override Response OnReceive(SimpleMessagePurport message)
		{
			return OnReceiveSimpleMessageStub?.Invoke(message);
		}
	}
}
