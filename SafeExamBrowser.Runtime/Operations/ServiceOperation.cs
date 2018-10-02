/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Core.OperationModel;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Runtime.Operations
{
	internal class ServiceOperation : IOperation
	{
		private bool connected, mandatory;
		private IConfigurationRepository configuration;
		private ILogger logger;
		private IServiceProxy service;

		public IProgressIndicator ProgressIndicator { private get; set; }

		public ServiceOperation(IConfigurationRepository configuration, ILogger logger, IServiceProxy service)
		{
			this.configuration = configuration;
			this.service = service;
			this.logger = logger;
		}

		public OperationResult Perform()
		{
			logger.Info($"Initializing service session...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_InitializeServiceSession);

			mandatory = configuration.CurrentSettings.ServicePolicy == ServicePolicy.Mandatory;
			connected = service.Connect();

			if (mandatory && !connected)
			{
				logger.Error("Aborting startup because the service is mandatory but not available!");

				return OperationResult.Failed;
			}

			service.Ignore = !connected;
			logger.Info($"The service is {(mandatory ? "mandatory" : "optional")} and {(connected ? "available." : "not available. All service-related operations will be ignored!")}");

			if (connected)
			{
				StartServiceSession();
			}

			return OperationResult.Success;
		}

		public OperationResult Repeat()
		{
			// TODO: Re-check if mandatory, if so, try to connect (if not connected) - otherwise, no action required (except maybe logging of status?)
			if (connected)
			{
				StopServiceSession();
				StartServiceSession();
			}

			return OperationResult.Success;
		}

		public void Revert()
		{
			logger.Info("Finalizing service session...");
			ProgressIndicator?.UpdateText(TextKey.ProgressIndicator_FinalizeServiceSession);

			if (connected)
			{
				StopServiceSession();

				var success = service.Disconnect();

				if (success)
				{
					logger.Info("Successfully disconnected from the service.");
				}
				else
				{
					logger.Error("Failed to disconnect from the service!");
				}
			}
		}

		private void StartServiceSession()
		{
			service.StartSession(configuration.CurrentSession.Id, configuration.CurrentSettings);
		}

		private void StopServiceSession()
		{
			service.StopSession(configuration.CurrentSession.Id);
		}
	}
}
