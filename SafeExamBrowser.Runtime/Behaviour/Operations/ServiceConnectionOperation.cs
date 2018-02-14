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
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Runtime.Behaviour.Operations
{
	internal class ServiceConnectionOperation : IOperation
	{
		private bool connected, mandatory;
		private IConfigurationRepository configuration;
		private ILogger logger;
		private IServiceProxy service;
		private IText text;

		public bool Abort { get; private set; }
		public IProgressIndicator ProgressIndicator { private get; set; }

		public ServiceConnectionOperation(IConfigurationRepository configuration, ILogger logger, IServiceProxy service, IText text)
		{
			this.configuration = configuration;
			this.service = service;
			this.logger = logger;
			this.text = text;
		}

		public void Perform()
		{
			logger.Info($"Initializing service connection...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_InitializeServiceConnection);

			try
			{
				mandatory = configuration.CurrentSettings.ServicePolicy == ServicePolicy.Mandatory;
				connected = service.Connect();
			}
			catch (Exception e)
			{
				LogException(e);
			}

			if (mandatory && !connected)
			{
				Abort = true;
				logger.Error("Aborting startup because the service is mandatory but not available!");
			}
			else
			{
				service.Ignore = !connected;
				logger.Info($"The service is {(mandatory ? "mandatory" : "optional")} and {(connected ? "available." : "not available. All service-related operations will be ignored!")}");
			}
		}

		public void Repeat()
		{
			// TODO
		}

		public void Revert()
		{
			logger.Info("Closing service connection...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_CloseServiceConnection);

			if (connected)
			{
				try
				{
					service.Disconnect();
				}
				catch (Exception e)
				{
					logger.Error("Failed to disconnect from the service!", e);
				}
			}
		}

		private void LogException(Exception e)
		{
			var message = "Failed to connect to the service component!";

			if (mandatory)
			{
				logger.Error(message, e);
			}
			else
			{
				logger.Info($"{message} Reason: {e.Message}");
			}
		}
	}
}
