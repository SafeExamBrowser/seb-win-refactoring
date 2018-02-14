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
		public Guid StartupToken { private get; set; }

		public ClientHost(string address, ILogger logger) : base(address, logger)
		{
		}

		protected override bool OnConnect(Guid? token)
		{
			return StartupToken == token;
		}

		protected override void OnDisconnect()
		{
			// TODO
		}

		protected override IResponse OnReceive(IMessage message)
		{
			// TODO
			return null;
		}

		protected override IResponse OnReceive(Message message)
		{
			// TODO
			return null;
		}
	}
}
