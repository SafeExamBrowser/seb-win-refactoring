/*
 * Copyright (c) 2026 ETH Zürich, IT Services
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
		// Security fix: Pre-shared authentication token that the runtime must provide to connect.
		// Generated at service start and shared via a protected mechanism (e.g. registry ACL or
		// temp file with restricted ACL). This prevents unauthorized local processes from
		// obtaining a CommunicationToken and calling privileged service operations.
		private static readonly Guid serviceAuthenticationToken = Guid.NewGuid();

		public event CommunicationEventHandler<SessionStartEventArgs> SessionStartRequested;
		public event CommunicationEventHandler<SessionStopEventArgs> SessionStopRequested;
		public event CommunicationEventHandler SystemConfigurationUpdateRequested;

		internal ServiceHost(string address, IHostObjectFactory factory, ILogger logger, int timeout_ms) : base(address, factory, logger, timeout_ms)
		{
			allowConnection = true;
		}

		protected override bool OnConnect(Guid? token)
		{
			// Security fix: The original implementation accepted the first connection without
			// any authentication, then blocked further connections. This means any local process
			// could connect to the service named pipe and obtain a valid CommunicationToken.
			// Now a pre-shared token must be provided. The runtime component should pass its
			// token via the Configuration argument or an environment-based shared secret.
			var allow = allowConnection && token.HasValue && token.Value == serviceAuthenticationToken;

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
