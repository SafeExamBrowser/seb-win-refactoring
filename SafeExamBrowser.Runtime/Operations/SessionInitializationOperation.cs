/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Communication.Contracts.Hosts;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class SessionInitializationOperation : SessionOperation
	{
		private IConfigurationRepository configuration;
		private IFileSystem fileSystem;
		private ILogger logger;
		private IRuntimeHost runtimeHost;

		public override event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public override event StatusChangedEventHandler StatusChanged;

		public SessionInitializationOperation(
			IConfigurationRepository configuration,
			IFileSystem fileSystem,
			ILogger logger,
			IRuntimeHost runtimeHost,
			SessionContext sessionContext) : base(sessionContext)
		{
			this.configuration = configuration;
			this.fileSystem = fileSystem;
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

			fileSystem.CreateDirectory(Context.Next.AppConfig.TemporaryDirectory);
		}

		private void FinalizeSessionConfiguration()
		{
			Context.Current = null;
			Context.Next = null;
		}
	}
}
