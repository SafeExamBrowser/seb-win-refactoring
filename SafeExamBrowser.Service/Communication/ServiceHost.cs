/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Communication.Hosts;
using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.Communication.Data;
using SafeExamBrowser.Contracts.Communication.Events;
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Service.Communication
{
	internal class ServiceHost : BaseHost, IServiceHost
	{
		private readonly object @lock = new object();

		private bool allowConnection;

		public bool AllowConnection
		{
			get { lock (@lock) { return allowConnection; } }
			set { lock (@lock) { allowConnection = value; } }
		}

		public event CommunicationEventHandler<SessionStartEventArgs> SessionStartRequested;
		public event CommunicationEventHandler<SessionStopEventArgs> SessionStopRequested;

		internal ServiceHost(string address, IHostObjectFactory factory, ILogger logger, int timeout_ms) : base(address, factory, logger, timeout_ms)
		{
			AllowConnection = true;
		}

		protected override bool OnConnect(Guid? token)
		{
			lock (@lock)
			{
				var allow = AllowConnection;

				if (allow)
				{
					AllowConnection = false;
				}

				return allow;
			}
		}

		protected override void OnDisconnect(Interlocutor interlocutor)
		{
			if (interlocutor == Interlocutor.Runtime)
			{
				lock (@lock)
				{
					AllowConnection = true;
				}
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
			return new SimpleResponse(SimpleResponsePurport.UnknownMessage);
		}
	}
}
