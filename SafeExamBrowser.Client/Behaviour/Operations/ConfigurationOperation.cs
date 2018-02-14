/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Behaviour.Operations;
using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Client.Behaviour.Operations
{
	internal class ConfigurationOperation : IOperation
	{
		private IClientConfiguration configuration;
		private ILogger logger;
		private IRuntimeProxy runtime;

		public bool Abort { get; private set; }
		public IProgressIndicator ProgressIndicator { private get; set; }

		public ConfigurationOperation(IClientConfiguration configuration, ILogger logger, IRuntimeProxy runtime)
		{
			this.configuration = configuration;
			this.logger = logger;
			this.runtime = runtime;
		}

		public void Perform()
		{
			logger.Info("Initializing application configuration...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_InitializeConfiguration);

			try
			{
				var config = runtime.GetConfiguration();

				configuration.RuntimeInfo = config.RuntimeInfo;
				configuration.SessionId = config.SessionId;
				configuration.Settings = config.Settings;

				logger.Info("Successfully retrieved the application configuration from the runtime.");
			}
			catch (Exception e)
			{
				logger.Error("An unexpected error occurred while trying to retrieve the application configuration!", e);
			}
		}

		public void Repeat()
		{
			// Nothing to do here...
		}

		public void Revert()
		{
			// Nothing to do here...
		}
	}
}
