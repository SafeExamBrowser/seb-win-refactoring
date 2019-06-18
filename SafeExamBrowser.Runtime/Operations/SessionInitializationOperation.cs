/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Core.OperationModel.Events;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class SessionInitializationOperation : SessionOperation
	{
		private IConfigurationRepository configuration;
		private ILogger logger;
		private IRuntimeHost runtimeHost;

		public override event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public override event StatusChangedEventHandler StatusChanged;

		public SessionInitializationOperation(
			IConfigurationRepository configuration,
			ILogger logger,
			IRuntimeHost runtimeHost,
			SessionContext sessionContext) : base(sessionContext)
		{
			this.configuration = configuration;
			this.logger = logger;
			this.runtimeHost = runtimeHost;
		}

		public override OperationResult Perform()
		{
			InitializeSessionConfiguration();

			return OperationResult.Success;
		}

		public override OperationResult Repeat()
		{
			InitializeSessionConfiguration();

			return OperationResult.Success;
		}

		public override OperationResult Revert()
		{
			FinalizeSessionConfiguration();

			return OperationResult.Success;
		}

		private void InitializeSessionConfiguration()
		{
			logger.Info("Initializing new session configuration...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeSession);

			Context.Next = configuration.InitializeSessionConfiguration();

			logger.Info($" -> Client-ID: {Context.Next.AppConfig.ClientId}");
			logger.Info($" -> Runtime-ID: {Context.Next.AppConfig.RuntimeId}");
			logger.Info($" -> Session-ID: {Context.Next.SessionId}");
		}

		private void FinalizeSessionConfiguration()
		{
			Context.Current = null;
			Context.Next = null;
		}
	}
}
