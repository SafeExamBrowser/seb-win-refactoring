/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Behaviour.Operations;
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
		private ILogger logger;
		private IRuntimeInfo runtimeInfo;
		private IRuntimeWindow runtimeWindow;
		private IServiceProxy serviceProxy;
		private ISettingsRepository settingsRepository;
		private IOperationSequence operationSequence;
		private Action terminationCallback;
		private IText text;
		private IUserInterfaceFactory uiFactory;

		public RuntimeController(
			ILogger logger,
			IOperationSequence operationSequence,
			IRuntimeInfo runtimeInfo,
			IServiceProxy serviceProxy,
			ISettingsRepository settingsRepository,
			Action terminationCallback,
			IText text,
			IUserInterfaceFactory uiFactory)
		{
			this.logger = logger;
			this.operationSequence = operationSequence;
			this.runtimeInfo = runtimeInfo;
			this.serviceProxy = serviceProxy;
			this.settingsRepository = settingsRepository;
			this.terminationCallback = terminationCallback;
			this.text = text;
			this.uiFactory = uiFactory;
		}

		public bool TryStart(Queue<IOperation> operations)
		{
			logger.Info("--- Initiating startup procedure ---");

			var success = operationSequence.TryPerform(operations);

			runtimeWindow = uiFactory.CreateRuntimeWindow(runtimeInfo, text);

			if (success)
			{
				logger.Info("--- Application successfully initialized! ---");
				logger.Log(string.Empty);
				logger.Subscribe(runtimeWindow);

				StartSession();
			}
			else
			{
				logger.Info("--- Application startup aborted! ---");
				logger.Log(string.Empty);
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

			terminationCallback.Invoke();
		}

		public void Terminate()
		{
			StopSession();

			// TODO:
			// - Disconnect from service
			// - Terminate runtime communication host
			// - Revert kiosk mode (or do that when stopping session?)

			logger.Unsubscribe(runtimeWindow);
			runtimeWindow.Close();

			logger.Log(string.Empty);
			logger.Info("--- Initiating shutdown procedure ---");

			var success = operationSequence.TryRevert();

			if (success)
			{
				logger.Info("--- Application successfully finalized! ---");
			}
			else
			{
				logger.Info("--- Shutdown procedure failed! ---");
			}
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
