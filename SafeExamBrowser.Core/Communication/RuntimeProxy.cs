/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Core.Communication.Messages;

namespace SafeExamBrowser.Core.Communication
{
	public class RuntimeProxy : BaseProxy, IRuntimeProxy
	{
		public RuntimeProxy(string address, ILogger logger) : base(address, logger)
		{
		}

		public bool Connect(Guid token)
		{
			return base.Connect(token).ConnectionEstablished;
		}

		public void Disconnect()
		{
			FailIfNotConnected(nameof(Disconnect));

			base.Disconnect(new Message { CommunicationToken = CommunicationToken.Value });
		}

		public IClientConfiguration GetConfiguration()
		{
			// TODO
			throw new NotImplementedException();
		}
	}
}
