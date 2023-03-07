/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Communication.Proxies;
using SafeExamBrowser.Communication.Contracts;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Communication.UnitTests.Proxies
{
	internal class BaseProxyImpl : BaseProxy
	{
		public BaseProxyImpl(string address, IProxyObjectFactory factory, ILogger logger, Interlocutor owner) : base(address, factory, logger, owner)
		{
		}

		public override bool Connect(Guid? token = null, bool autoPing = false)
		{
			return base.Connect(token, autoPing);
		}

		public override bool Disconnect()
		{
			return base.Disconnect();
		}

		public new Response Send(Message message)
		{
			return base.Send(message);
		}

		public new Response Send(SimpleMessagePurport purport)
		{
			return base.Send(purport);
		}

		public new bool IsAcknowledged(Response response)
		{
			return base.IsAcknowledged(response);
		}

		public new void TestConnection()
		{
			base.TestConnection();
		}

		public new string ToString(Message message)
		{
			return base.ToString(message);
		}

		public new string ToString(Response response)
		{
			return base.ToString(response);
		}
	}
}
