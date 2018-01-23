/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Runtime.Behaviour.Operations
{
	internal class ServiceOperation : IOperation
	{
		private ICommunicationHost serviceHost;
		private ILogger logger;
		private ISettingsRepository settingsRepository;

		public bool AbortStartup { get; private set; }
		public ISplashScreen SplashScreen { private get; set; }

		public ServiceOperation(ICommunicationHost serviceHost, ILogger logger, ISettingsRepository settingsRepository)
		{
			this.serviceHost = serviceHost;
			this.logger = logger;
			this.settingsRepository = settingsRepository;
		}

		public void Perform()
		{
			logger.Info("Initializing service connection...");
			// SplashScreen.UpdateText(...)

			// TODO
		}

		public void Revert()
		{
			// TODO
		}
	}
}
