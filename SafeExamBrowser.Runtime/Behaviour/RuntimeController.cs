/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Communication;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Runtime.Behaviour
{
	internal class RuntimeController : IRuntimeController
	{
		private Queue<IOperation> operations;
		private ILogger logger;
		private IRuntimeInfo runtimeInfo;
		private IRuntimeWindow runtimeWindow;
		private IServiceProxy serviceProxy;
		private ISettingsRepository settingsRepository;
		private IShutdownController shutdownController;
		private IStartupController startupController;
		private Action terminationCallback;
		private IText text;
		private IUserInterfaceFactory uiFactory;

		public RuntimeController(
			ILogger logger,
			IRuntimeInfo runtimeInfo,
			IServiceProxy serviceProxy,
			ISettingsRepository settingsRepository,
			IShutdownController shutdownController,
			IStartupController startupController,
			Action terminationCallback,
			IText text,
			IUserInterfaceFactory uiFactory)
		{
			this.logger = logger;
			this.runtimeInfo = runtimeInfo;
			this.serviceProxy = serviceProxy;
			this.settingsRepository = settingsRepository;
			this.shutdownController = shutdownController;
			this.startupController = startupController;
			this.terminationCallback = terminationCallback;
			this.text = text;
			this.uiFactory = uiFactory;

			operations = new Queue<IOperation>();
		}

		public bool TryInitializeApplication(Queue<IOperation> operations)
		{
			var success = startupController.TryInitializeApplication(operations);

			runtimeWindow = uiFactory.CreateRuntimeWindow(runtimeInfo, text);

			if (success)
			{
				this.operations = new Queue<IOperation>(operations);
				logger.Subscribe(runtimeWindow);
			}

			return success;
		}

		public void StartSession()
		{
			runtimeWindow.Show();

			logger.Info("Starting new session...");
			runtimeWindow.UpdateStatus(TextKey.RuntimeWindow_StartSession, true);

			// TODO:
			// - Initialize configuration
			// - Initialize kiosk mode
			// - Initialize session data
			// - Start runtime communication host
			// - Create and connect to client
			// - Initialize session with service
			// - Verify session integrity and start event handling
			System.Threading.Thread.Sleep(10000);

			runtimeWindow.UpdateStatus(TextKey.RuntimeWindow_ApplicationRunning);

			if (settingsRepository.Current.KioskMode == KioskMode.DisableExplorerShell)
			{
				runtimeWindow.Hide();
			}
		}

		public void FinalizeApplication()
		{
			StopSession();

			// TODO:
			// - Disconnect from service
			// - Terminate runtime communication host
			// - Revert kiosk mode (or do that when stopping session?)

			logger.Unsubscribe(runtimeWindow);
			runtimeWindow.Close();
			shutdownController.FinalizeApplication(new Queue<IOperation>(operations.Reverse()));
		}

		private void StopSession()
		{
			logger.Info("Stopping current session...");
			runtimeWindow.Show();
			runtimeWindow.BringToForeground();
			runtimeWindow.UpdateStatus(TextKey.RuntimeWindow_StopSession, true);

			// TODO:
			// - Terminate client (or does it terminate itself?)
			// - Finalize session with service
			// - Stop event handling and close session
		}
	}
}
