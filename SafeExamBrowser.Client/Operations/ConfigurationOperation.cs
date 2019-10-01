/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Communication.Contracts.Proxies;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Client.Operations
{
	internal class ConfigurationOperation : ClientOperation
	{
		private ClientConfiguration configuration;
		private ILogger logger;
		private IRuntimeProxy runtime;

		public override event ActionRequiredEventHandler ActionRequired { add { } remove { } }
		public override event StatusChangedEventHandler StatusChanged;

		// TODO: Remove and delete ClientConfiguration!
		public ConfigurationOperation(ClientConfiguration configuration, ClientContext context, ILogger logger, IRuntimeProxy runtime) : base(context)
		{
			this.configuration = configuration;
			this.logger = logger;
			this.runtime = runtime;
		}

		public override OperationResult Perform()
		{
			logger.Info("Initializing application configuration...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeConfiguration);

			var communication = runtime.GetConfiguration();
			var config = communication.Value.Configuration;

			configuration.AppConfig = config.AppConfig;
			configuration.SessionId = config.SessionId;
			configuration.Settings = config.Settings;

			Context.AppConfig = config.AppConfig;
			Context.Settings = config.Settings;

			logger.Info("Successfully retrieved the application configuration from the runtime.");
			logger.Info($" -> Client-ID: {configuration.AppConfig.ClientId}");
			logger.Info($" -> Runtime-ID: {configuration.AppConfig.RuntimeId}");
			logger.Info($" -> Session-ID: {configuration.SessionId}");

			return OperationResult.Success;
		}

		public override OperationResult Revert()
		{
			return OperationResult.Success;
		}
	}
}
