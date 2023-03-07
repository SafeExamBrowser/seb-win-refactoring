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

namespace SafeExamBrowser.Runtime.Communication
{
	internal class RuntimeHost : BaseHost, IRuntimeHost
	{
		public bool AllowConnection { get; set; }
		public Guid? AuthenticationToken { private get; set; }

		public event CommunicationEventHandler ClientDisconnected;
		public event CommunicationEventHandler ClientReady;
		public event CommunicationEventHandler<ClientConfigurationEventArgs> ClientConfigurationNeeded;
		public event CommunicationEventHandler<ExamSelectionReplyEventArgs> ExamSelectionReceived;
		public event CommunicationEventHandler<MessageBoxReplyEventArgs> MessageBoxReplyReceived;
		public event CommunicationEventHandler<PasswordReplyEventArgs> PasswordReceived;
		public event CommunicationEventHandler<ReconfigurationEventArgs> ReconfigurationRequested;
		public event CommunicationEventHandler<ServerFailureActionReplyEventArgs> ServerFailureActionReceived;
		public event CommunicationEventHandler ShutdownRequested;

		public RuntimeHost(string address, IHostObjectFactory factory, ILogger logger, int timeout_ms) : base(address, factory, logger, timeout_ms)
		{
		}

		protected override bool OnConnect(Guid? token = null)
		{
			var authenticated = AuthenticationToken.HasValue && AuthenticationToken == token;
			var accepted = AllowConnection && authenticated;

			if (accepted)
			{
				AllowConnection = false;
			}

			return accepted;
		}

		protected override void OnDisconnect(Interlocutor interlocutor)
		{
			if (interlocutor == Interlocutor.Client)
			{
				ClientDisconnected?.Invoke();
			}
		}

		protected override Response OnReceive(Message message)
		{
			switch (message)
			{
				case ExamSelectionReplyMessage m:
					ExamSelectionReceived?.InvokeAsync(new ExamSelectionReplyEventArgs { RequestId = m.RequestId, SelectedExamId = m.SelectedExamId, Success = m.Success });
					return new SimpleResponse(SimpleResponsePurport.Acknowledged);
				case MessageBoxReplyMessage m:
					MessageBoxReplyReceived?.InvokeAsync(new MessageBoxReplyEventArgs { RequestId = m.RequestId, Result = m.Result });
					return new SimpleResponse(SimpleResponsePurport.Acknowledged);
				case PasswordReplyMessage m:
					PasswordReceived?.InvokeAsync(new PasswordReplyEventArgs { Password = m.Password, RequestId = m.RequestId, Success = m.Success });
					return new SimpleResponse(SimpleResponsePurport.Acknowledged);
				case ReconfigurationMessage m:
					ReconfigurationRequested?.InvokeAsync(new ReconfigurationEventArgs { ConfigurationPath = m.ConfigurationPath, ResourceUrl = m.ResourceUrl });
					return new SimpleResponse(SimpleResponsePurport.Acknowledged);
				case ServerFailureActionReplyMessage m:
					ServerFailureActionReceived?.InvokeAsync(new ServerFailureActionReplyEventArgs { Abort = m.Abort, Fallback = m.Fallback, RequestId = m.RequestId, Retry = m.Retry });
					return new SimpleResponse(SimpleResponsePurport.Acknowledged);
			}

			return new SimpleResponse(SimpleResponsePurport.UnknownMessage);
		}

		protected override Response OnReceive(SimpleMessagePurport message)
		{
			switch (message)
			{
				case SimpleMessagePurport.ClientIsReady:
					ClientReady?.Invoke();
					return new SimpleResponse(SimpleResponsePurport.Acknowledged);
				case SimpleMessagePurport.ConfigurationNeeded:
					return HandleConfigurationRequest();
				case SimpleMessagePurport.RequestShutdown:
					ShutdownRequested?.Invoke();
					return new SimpleResponse(SimpleResponsePurport.Acknowledged);
			}

			return new SimpleResponse(SimpleResponsePurport.UnknownMessage);
		}

		private Response HandleConfigurationRequest()
		{
			var args = new ClientConfigurationEventArgs();

			ClientConfigurationNeeded?.Invoke(args);

			return new ConfigurationResponse { Configuration = args.ClientConfiguration };
		}
	}
}
