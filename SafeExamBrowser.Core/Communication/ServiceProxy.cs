/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Core.Communication.Messages;

namespace SafeExamBrowser.Core.Communication
{
	public class ServiceProxy : BaseProxy, IServiceProxy
	{
		public bool Ignore { private get; set; }

		public ServiceProxy(string address, ILogger logger) : base(address, logger)
		{
		}

		public bool Connect()
		{
			if (!IgnoreOperation(nameof(Connect)))
			{
				return base.Connect().ConnectionEstablished;
			}

			return false;
		}

		public void Disconnect()
		{
			if (!IgnoreOperation(nameof(Disconnect)))
			{
				FailIfNotConnected(nameof(Disconnect));

				Disconnect(new Message { CommunicationToken = CommunicationToken.Value });
			}
		}

		private bool IgnoreOperation(string operationName)
		{
			if (Ignore)
			{
				Logger.Debug($"Skipping {operationName} because the ignore flag is set.");
			}

			return Ignore;
		}
	}
}
