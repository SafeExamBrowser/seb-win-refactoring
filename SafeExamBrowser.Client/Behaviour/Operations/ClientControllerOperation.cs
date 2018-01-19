/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Client;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Client.Behaviour.Operations
{
	internal class ClientControllerOperation : IOperation
	{
		private ILogger logger;
		private IClientController controller;

		public bool AbortStartup { get; private set; }
		public ISplashScreen SplashScreen { private get; set; }

		public ClientControllerOperation(IClientController controller, ILogger logger)
		{
			this.controller = controller;
			this.logger = logger;
		}

		public void Perform()
		{
			logger.Info("Starting event handling...");
			SplashScreen.UpdateText(TextKey.SplashScreen_StartEventHandling);

			controller.Start();
		}

		public void Revert()
		{
			logger.Info("Stopping event handling...");
			SplashScreen.UpdateText(TextKey.SplashScreen_StopEventHandling);

			controller.Stop();
		}
	}
}
