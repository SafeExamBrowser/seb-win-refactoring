/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Communication.Contracts.Events;
using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Runtime.Responsibilities
{
	internal class CommunicationResponsibility : RuntimeResponsibility
	{
		private readonly IRuntimeHost runtimeHost;
		private readonly Action shutdown;

		internal CommunicationResponsibility(
			ILogger logger,
			RuntimeContext runtimeContext,
			IRuntimeHost runtimeHost,
			Action shutdown) : base(logger, runtimeContext)
		{
			this.runtimeHost = runtimeHost;
			this.shutdown = shutdown;
		}

		public override void Assume(RuntimeTask task)
		{
			switch (task)
			{
				case RuntimeTask.DeregisterEvents:
					DeregisterEvents();
					break;
				case RuntimeTask.RegisterEvents:
					RegisterEvents();
					break;
			}
		}

		private void RegisterEvents()
		{
			runtimeHost.ClientConfigurationNeeded += RuntimeHost_ClientConfigurationNeeded;
			runtimeHost.ReconfigurationRequested += RuntimeHost_ReconfigurationRequested;
			runtimeHost.ShutdownRequested += RuntimeHost_ShutdownRequested;
		}

		private void DeregisterEvents()
		{
			runtimeHost.ClientConfigurationNeeded -= RuntimeHost_ClientConfigurationNeeded;
			runtimeHost.ReconfigurationRequested -= RuntimeHost_ReconfigurationRequested;
			runtimeHost.ShutdownRequested -= RuntimeHost_ShutdownRequested;
		}

		private void RuntimeHost_ClientConfigurationNeeded(ClientConfigurationEventArgs args)
		{
			args.ClientConfiguration = new ClientConfiguration
			{
				AppConfig = Context.Next.AppConfig,
				SessionId = Context.Next.SessionId,
				Settings = Context.Next.Settings
			};
		}

		private void RuntimeHost_ReconfigurationRequested(ReconfigurationEventArgs args)
		{
			Logger.Info($"Accepted request for reconfiguration with '{args.ConfigurationPath}'.");

			Context.ReconfigurationFilePath = args.ConfigurationPath;
			Context.ReconfigurationUrl = args.ResourceUrl;
			Context.Responsibilities.Delegate(RuntimeTask.StartSession);
		}

		private void RuntimeHost_ShutdownRequested()
		{
			Logger.Info("Received shutdown request from the client application.");
			shutdown.Invoke();
		}
	}
}
