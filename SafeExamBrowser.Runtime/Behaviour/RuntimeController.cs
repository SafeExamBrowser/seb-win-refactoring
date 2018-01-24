/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Linq;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.Logging;

namespace SafeExamBrowser.Runtime.Behaviour
{
	internal class RuntimeController : IRuntimeController
	{
		private ICommunicationHost serviceProxy;
		private Queue<IOperation> operations;
		private ILogger logger;
		private ISettingsRepository settingsRepository;
		private IShutdownController shutdownController;
		private IStartupController startupController;

		public ISettings Settings { private get; set; }

		public RuntimeController(
			ICommunicationHost serviceProxy,
			ILogger logger,
			ISettingsRepository settingsRepository,
			IShutdownController shutdownController,
			IStartupController startupController)
		{
			this.serviceProxy = serviceProxy;
			this.logger = logger;
			this.settingsRepository = settingsRepository;
			this.shutdownController = shutdownController;
			this.startupController = startupController;

			operations = new Queue<IOperation>();
		}

		public bool TryInitializeApplication(Queue<IOperation> operations)
		{
			operations = new Queue<IOperation>(operations);

			var success = startupController.TryInitializeApplication(operations);

			if (success)
			{
				Start();
			}

			return success;
		}

		public void FinalizeApplication()
		{
			Stop();
			shutdownController.FinalizeApplication(new Queue<IOperation>(operations.Reverse()));
		}

		private void Start()
		{
			logger.Info("Starting event handling...");
			// TODO SplashScreen.UpdateText(TextKey.SplashScreen_StartEventHandling);
		}

		private void Stop()
		{
			logger.Info("Stopping event handling...");
			// TODO SplashScreen.UpdateText(TextKey.SplashScreen_StopEventHandling);
		}
	}
}
