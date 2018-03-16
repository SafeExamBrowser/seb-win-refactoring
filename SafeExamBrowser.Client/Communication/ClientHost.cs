/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.Communication.Data;
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Core.Communication.Hosts;

namespace SafeExamBrowser.Client.Communication
{
	internal class ClientHost : BaseHost, IClientHost
	{
		private bool allowConnection = true;
		private int processId;

		public Guid StartupToken { private get; set; }

		public event CommunicationEventHandler Shutdown;

		public ClientHost(string address, IHostObjectFactory factory, ILogger logger, int processId) : base(address, factory, logger)
		{
			this.processId = processId;
		}

		protected override bool OnConnect(Guid? token)
		{
			var authenticated = StartupToken == token;
			var accepted = allowConnection && authenticated;

			if (accepted)
			{
				allowConnection = false;
			}

			return accepted;
		}

		protected override void OnDisconnect()
		{
			// Nothing to do here...
		}

		protected override Response OnReceive(Message message)
		{
			return new SimpleResponse(SimpleResponsePurport.UnknownMessage);
		}

		protected override Response OnReceive(SimpleMessagePurport message)
		{
			switch (message)
			{
				case SimpleMessagePurport.Authenticate:
					return new AuthenticationResponse { ProcessId = processId };
				case SimpleMessagePurport.Shutdown:
					Shutdown?.Invoke();
					return new SimpleResponse(SimpleResponsePurport.Acknowledged);
			}

			return new SimpleResponse(SimpleResponsePurport.UnknownMessage);
		}
	}
}
