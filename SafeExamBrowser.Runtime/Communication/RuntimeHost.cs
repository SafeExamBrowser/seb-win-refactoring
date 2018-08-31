/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Communication.Data;
using SafeExamBrowser.Contracts.Communication.Events;
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Communication.Hosts;

namespace SafeExamBrowser.Runtime.Communication
{
	internal class RuntimeHost : BaseHost, IRuntimeHost
	{
		private bool allowConnection = true;
		private IConfigurationRepository configuration;

		public Guid StartupToken { private get; set; }

		public event CommunicationEventHandler ClientDisconnected;
		public event CommunicationEventHandler ClientReady;
		public event CommunicationEventHandler<PasswordReplyEventArgs> PasswordReceived;
		public event CommunicationEventHandler<ReconfigurationEventArgs> ReconfigurationRequested;
		public event CommunicationEventHandler ShutdownRequested;

		public RuntimeHost(string address, IConfigurationRepository configuration, IHostObjectFactory factory, ILogger logger) : base(address, factory, logger)
		{
			this.configuration = configuration;
		}

		protected override bool OnConnect(Guid? token = null)
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
			ClientDisconnected?.Invoke();
			// TODO: Handle client crash scenario!
			// If a client crashes or hangs when terminating (which should not happen!), it could be that it never gets to disconnect from
			// the RuntimeHost - in that case, allowConnection prohibits restarting a new session as long as it's only set here!
			//		-> Move AllowConnection to interface and reset it in SessionController?
			//		-> Only possible as long as just the client connects, with service and client a more elaborate solution will be needed!
			//			-> E.g. ClientId and ServiceId, and then AllowClientConnection and AllowServiceConnection?
			allowConnection = true;
		}

		protected override Response OnReceive(Message message)
		{
			switch (message)
			{
				case PasswordReplyMessage r:
					PasswordReceived?.InvokeAsync(new PasswordReplyEventArgs { Password = r.Password, RequestId = r.RequestId, Success = r.Success });
					return new SimpleResponse(SimpleResponsePurport.Acknowledged);
				case ReconfigurationMessage r:
					ReconfigurationRequested?.InvokeAsync(new ReconfigurationEventArgs { ConfigurationPath = r.ConfigurationPath });
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
					// TODO: Not the job of the host, fire event or alike!
					return new ConfigurationResponse { Configuration = configuration.BuildClientConfiguration() };
				case SimpleMessagePurport.RequestShutdown:
					ShutdownRequested?.Invoke();
					return new SimpleResponse(SimpleResponsePurport.Acknowledged);
			}

			return new SimpleResponse(SimpleResponsePurport.UnknownMessage);
		}
	}
}
