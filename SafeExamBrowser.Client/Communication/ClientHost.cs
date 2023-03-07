/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Communication.Contracts;
using SafeExamBrowser.Communication.Contracts.Data;
using SafeExamBrowser.Communication.Contracts.Events;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Communication.Hosts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Client.Communication
{
	internal class ClientHost : BaseHost, IClientHost
	{
		private bool allowConnection;
		private int processId;

		public Guid AuthenticationToken { private get; set; }
		public bool IsConnected { get; private set; }

		public event CommunicationEventHandler<ExamSelectionRequestEventArgs> ExamSelectionRequested;
		public event CommunicationEventHandler<MessageBoxRequestEventArgs> MessageBoxRequested;
		public event CommunicationEventHandler<PasswordRequestEventArgs> PasswordRequested;
		public event CommunicationEventHandler ReconfigurationAborted;
		public event CommunicationEventHandler<ReconfigurationEventArgs> ReconfigurationDenied;
		public event CommunicationEventHandler RuntimeDisconnected;
		public event CommunicationEventHandler<ServerFailureActionRequestEventArgs> ServerFailureActionRequested;
		public event CommunicationEventHandler Shutdown;

		public ClientHost(
			string address,
			IHostObjectFactory factory,
			ILogger logger,
			int processId,
			int timeout_ms) : base(address, factory, logger, timeout_ms)
		{
			this.allowConnection = true;
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

		protected override void OnDisconnect(Interlocutor interlocutor)
		{
			if (interlocutor == Interlocutor.Runtime)
			{
				RuntimeDisconnected?.Invoke();
				IsConnected = false;
			}
		}

		protected override Response OnReceive(Message message)
		{
			switch (message)
			{
				case ExamSelectionRequestMessage m:
					ExamSelectionRequested?.InvokeAsync(new ExamSelectionRequestEventArgs { Exams = m.Exams, RequestId = m.RequestId });
					return new SimpleResponse(SimpleResponsePurport.Acknowledged);
				case MessageBoxRequestMessage m:
					MessageBoxRequested?.InvokeAsync(new MessageBoxRequestEventArgs { Action = m.Action, Icon = m.Icon, Message = m.Message, RequestId = m.RequestId, Title = m.Title });
					return new SimpleResponse(SimpleResponsePurport.Acknowledged);
				case PasswordRequestMessage m:
					PasswordRequested?.InvokeAsync(new PasswordRequestEventArgs { Purpose = m.Purpose, RequestId = m.RequestId });
					return new SimpleResponse(SimpleResponsePurport.Acknowledged);
				case ReconfigurationDeniedMessage m:
					ReconfigurationDenied?.InvokeAsync(new ReconfigurationEventArgs { ConfigurationPath = m.FilePath });
					return new SimpleResponse(SimpleResponsePurport.Acknowledged);
				case ServerFailureActionRequestMessage m:
					ServerFailureActionRequested?.InvokeAsync(new ServerFailureActionRequestEventArgs { Message = m.Message, RequestId = m.RequestId, ShowFallback = m.ShowFallback });
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
				case SimpleMessagePurport.ReconfigurationAborted:
					ReconfigurationAborted?.InvokeAsync();
					return new SimpleResponse(SimpleResponsePurport.Acknowledged);
				case SimpleMessagePurport.Shutdown:
					Shutdown?.Invoke();
					return new SimpleResponse(SimpleResponsePurport.Acknowledged);
			}

			return new SimpleResponse(SimpleResponsePurport.UnknownMessage);
		}
	}
}
