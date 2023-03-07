/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Communication.Hosts;
using SafeExamBrowser.Communication.Contracts;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Communication.Contracts.Events;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Service.Communication
{
	internal class ServiceHost : BaseHost, IServiceHost
	{
		private bool allowConnection;

		public event CommunicationEventHandler<SessionStartEventArgs> SessionStartRequested;
		public event CommunicationEventHandler<SessionStopEventArgs> SessionStopRequested;
		public event CommunicationEventHandler SystemConfigurationUpdateRequested;

		internal ServiceHost(string address, IHostObjectFactory factory, ILogger logger, int timeout_ms) : base(address, factory, logger, timeout_ms)
		{
			allowConnection = true;
		}

		protected override bool OnConnect(Guid? token)
		{
			var allow = allowConnection;

			if (allow)
			{
				allowConnection = false;
			}

			return allow;
		}

		protected override void OnDisconnect(Interlocutor interlocutor)
		{
			if (interlocutor == Interlocutor.Runtime)
			{
				allowConnection = true;
			}
		}

		protected override Response OnReceive(Message message)
		{
			switch (message)
			{
				case SessionStartMessage m:
					SessionStartRequested?.InvokeAsync(new SessionStartEventArgs { Configuration = m.Configuration });
					return new SimpleResponse(SimpleResponsePurport.Acknowledged);
				case SessionStopMessage m:
					SessionStopRequested?.InvokeAsync(new SessionStopEventArgs { SessionId = m.SessionId });
					return new SimpleResponse(SimpleResponsePurport.Acknowledged);
			}

			return new SimpleResponse(SimpleResponsePurport.UnknownMessage);
		}

		protected override Response OnReceive(SimpleMessagePurport message)
		{
			switch (message)
			{
				case SimpleMessagePurport.UpdateSystemConfiguration:
					SystemConfigurationUpdateRequested?.InvokeAsync();
					return new SimpleResponse(SimpleResponsePurport.Acknowledged);
			}

			return new SimpleResponse(SimpleResponsePurport.UnknownMessage);
		}
	}
}
