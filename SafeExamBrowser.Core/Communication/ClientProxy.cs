/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.Communication.Messages;
using SafeExamBrowser.Contracts.Communication.Responses;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Core.Communication
{
	public class ClientProxy : BaseProxy, IClientProxy
	{
		public ClientProxy(string address, ILogger logger) : base(address, logger)
		{
		}

		public AuthenticationResponse RequestAuthentication()
		{
			return (AuthenticationResponse) Send(MessagePurport.ClientIsReady);
		}
	}
}
