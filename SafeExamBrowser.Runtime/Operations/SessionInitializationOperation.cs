/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
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
	internal class SessionInitializationOperation : IRepeatableOperation
	{
		private IConfigurationRepository configuration;
		private ILogger logger;
		private IRuntimeHost runtimeHost;

		public event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public event StatusChangedEventHandler StatusChanged;

		public SessionInitializationOperation(IConfigurationRepository configuration, ILogger logger, IRuntimeHost runtimeHost)
		{
			this.configuration = configuration;
			this.logger = logger;
			this.runtimeHost = runtimeHost;
		}

		public OperationResult Perform()
		{
			InitializeSessionConfiguration();

			return OperationResult.Success;
		}

		public OperationResult Repeat()
		{
			return Perform();
		}

		public OperationResult Revert()
		{
			return OperationResult.Success;
		}

		private void InitializeSessionConfiguration()
		{
			logger.Info("Initializing new session configuration...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeSession);

			configuration.InitializeSessionConfiguration();
			runtimeHost.StartupToken = configuration.CurrentSession.StartupToken;

			logger.Info($" -> Client-ID: {configuration.AppConfig.ClientId}");
			logger.Info($" -> Runtime-ID: {configuration.AppConfig.RuntimeId}");
			logger.Info($" -> Session-ID: {configuration.CurrentSession.Id}");
		}
	}
}
