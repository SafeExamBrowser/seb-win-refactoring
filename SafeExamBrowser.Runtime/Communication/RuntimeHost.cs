/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.Communication.Messages;
using SafeExamBrowser.Contracts.Communication.Responses;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Core.Communication;

namespace SafeExamBrowser.Runtime.Communication
{
	internal class RuntimeHost : BaseHost, IRuntimeHost
	{
		private bool allowConnection = true;
		private IConfigurationRepository configuration;

		public Guid StartupToken { private get; set; }

		public event CommunicationEventHandler ClientDisconnected;
		public event CommunicationEventHandler ClientReady;
		public event CommunicationEventHandler ShutdownRequested;

		public RuntimeHost(string address, IConfigurationRepository configuration, ILogger logger) : base(address, logger)
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
		}

		protected override Response OnReceive(Message message)
		{
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
					return new ConfigurationResponse { Configuration = configuration.BuildClientConfiguration() };
				case SimpleMessagePurport.RequestShutdown:
					ShutdownRequested?.Invoke();
					return new SimpleResponse(SimpleResponsePurport.Acknowledged);
			}

			return new SimpleResponse(SimpleResponsePurport.UnknownMessage);
		}
	}
}
