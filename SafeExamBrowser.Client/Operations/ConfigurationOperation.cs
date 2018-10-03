/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.Core.OperationModel.Events;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Client.Operations
{
	internal class ConfigurationOperation : IOperation
	{
		private ClientConfiguration configuration;
		private ILogger logger;
		private IRuntimeProxy runtime;

		public event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public event StatusChangedEventHandler StatusChanged;

		public ConfigurationOperation(ClientConfiguration configuration, ILogger logger, IRuntimeProxy runtime)
		{
			this.configuration = configuration;
			this.logger = logger;
			this.runtime = runtime;
		}

		public OperationResult Perform()
		{
			logger.Info("Initializing application configuration...");
			StatusChanged?.Invoke(TextKey.ProgressIndicator_InitializeConfiguration);

			try
			{
				var communication = runtime.GetConfiguration();
				var config = communication.Value.Configuration;

				configuration.AppConfig = config.AppConfig;
				configuration.SessionId = config.SessionId;
				configuration.Settings = config.Settings;

				logger.Info("Successfully retrieved the application configuration from the runtime.");
				logger.Info($" -> Client-ID: {configuration.AppConfig.ClientId}");
				logger.Info($" -> Runtime-ID: {configuration.AppConfig.RuntimeId}");
				logger.Info($" -> Session-ID: {configuration.SessionId}");
			}
			catch (Exception e)
			{
				logger.Error("An unexpected error occurred while trying to retrieve the application configuration!", e);

				return OperationResult.Failed;
			}

			return OperationResult.Success;
		}

		public OperationResult Repeat()
		{
			return OperationResult.Success;
		}

		public void Revert()
		{
			// Nothing to do here...
		}
	}
}
