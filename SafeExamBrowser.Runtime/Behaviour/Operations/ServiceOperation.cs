/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Runtime.Behaviour.Operations
{
	internal class ServiceOperation : IOperation
	{
		private bool serviceAvailable;
		private bool serviceMandatory;
		private ILogger logger;
		private IServiceProxy service;
		private ISettingsRepository settingsRepository;
		private IText text;

		public bool AbortStartup { get; private set; }
		public ISplashScreen SplashScreen { private get; set; }

		public ServiceOperation(ILogger logger, IServiceProxy service, ISettingsRepository settingsRepository, IText text)
		{
			this.service = service;
			this.logger = logger;
			this.settingsRepository = settingsRepository;
			this.text = text;
		}

		public void Perform()
		{
			logger.Info($"Initializing service connection...");
			SplashScreen.UpdateText(TextKey.SplashScreen_InitializeServiceConnection);

			try
			{
				serviceMandatory = settingsRepository.Current.ServicePolicy == ServicePolicy.Mandatory;
				serviceAvailable = service.Connect();
			}
			catch (Exception e)
			{
				var message = "Failed to connect to the service component!";

				if (serviceMandatory)
				{
					logger.Error(message, e);
				}
				else
				{
					logger.Info($"{message} Reason: {e.Message}");
				}
			}

			AbortStartup = serviceMandatory && !serviceAvailable;

			if (AbortStartup)
			{
				logger.Info("Aborting startup because the service is mandatory but not available!");
			}
			else
			{
				logger.Info($"The service is {(serviceMandatory ? "mandatory" : "optional")} and {(!serviceAvailable ? "not" : "")} available.");
			}
		}

		public void Revert()
		{
			logger.Info("Closing service connection...");
			SplashScreen.UpdateText(TextKey.SplashScreen_CloseServiceConnection);

			if (serviceAvailable)
			{
				try
				{
					service.Disconnect();
				}
				catch (Exception e)
				{
					logger.Error("Failed to disconnect from service component!", e);
				}
			}
		}
	}
}
