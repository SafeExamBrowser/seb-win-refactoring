/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Communication.Hosts;
using SafeExamBrowser.Contracts.Communication.Data;
using SafeExamBrowser.Contracts.Communication.Events;
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Client.Communication
{
	internal class ClientHost : BaseHost, IClientHost
	{
		private bool allowConnection = true;
		private int processId;

		public Guid AuthenticationToken { private get; set; }
		public bool IsConnected { get; private set; }

		public event CommunicationEventHandler<MessageBoxRequestEventArgs> MessageBoxRequested;
		public event CommunicationEventHandler<PasswordRequestEventArgs> PasswordRequested;
		public event CommunicationEventHandler<ReconfigurationEventArgs> ReconfigurationDenied;
		public event CommunicationEventHandler RuntimeDisconnected;
		public event CommunicationEventHandler Shutdown;

		public ClientHost(
			string address,
			IHostObjectFactory factory,
			ILogger logger,
			int processId,
			int timeout_ms) : base(address, factory, logger, timeout_ms)
		{
			this.processId = processId;
		}

		protected override bool OnConnect(Guid? token)
		{
			var authenticated = AuthenticationToken == token;
			var accepted = allowConnection && authenticated;

			if (accepted)
			{
				allowConnection = false;
				IsConnected = true;
			}

			return accepted;
		}

		protected override void OnDisconnect()
		{
			RuntimeDisconnected?.Invoke();
			IsConnected = false;
		}

		protected override Response OnReceive(Message message)
		{
			switch (message)
			{
				case MessageBoxRequestMessage m:
					MessageBoxRequested?.InvokeAsync(new MessageBoxRequestEventArgs { Action = m.Action, Icon = m.Icon, Message = m.Message, RequestId = m.RequestId, Title = m.Title });
					return new SimpleResponse(SimpleResponsePurport.Acknowledged);
				case PasswordRequestMessage m:
					PasswordRequested?.InvokeAsync(new PasswordRequestEventArgs { Purpose = m.Purpose, RequestId = m.RequestId });
					return new SimpleResponse(SimpleResponsePurport.Acknowledged);
				case ReconfigurationDeniedMessage m:
					ReconfigurationDenied?.InvokeAsync(new ReconfigurationEventArgs { ConfigurationPath = m.FilePath });
					return new SimpleResponse(SimpleResponsePurport.Acknowledged);
			}

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
