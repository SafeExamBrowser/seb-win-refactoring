/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
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
		private bool initialized;

		private IConfigurationRepository configuration;
		private ILogger logger;
		private IOperationSequence bootstrapSequence;
		private IOperationSequence sessionSequence;
		private IRuntimeHost runtimeHost;
		private IRuntimeInfo runtimeInfo;
		private IRuntimeWindow runtimeWindow;
		private IServiceProxy serviceProxy;
		private ISplashScreen splashScreen;
		private Action shutdown;
		private IUserInterfaceFactory uiFactory;
		
		public RuntimeController(
			IConfigurationRepository configuration,
			ILogger logger,
			IOperationSequence bootstrapSequence,
			IOperationSequence sessionSequence,
			IRuntimeHost runtimeHost,
			IRuntimeInfo runtimeInfo,
			IServiceProxy serviceProxy,
			Action shutdown,
			IUserInterfaceFactory uiFactory)
		{
			this.configuration = configuration;
			this.logger = logger;
			this.bootstrapSequence = bootstrapSequence;
			this.sessionSequence = sessionSequence;
			this.runtimeHost = runtimeHost;
			this.runtimeInfo = runtimeInfo;
			this.serviceProxy = serviceProxy;
			this.shutdown = shutdown;
			this.uiFactory = uiFactory;
		}

		public bool TryStart()
		{
			logger.Info("--- Initiating startup procedure ---");

			runtimeWindow = uiFactory.CreateRuntimeWindow(runtimeInfo);
			splashScreen = uiFactory.CreateSplashScreen(runtimeInfo);

			bootstrapSequence.ProgressIndicator = splashScreen;
			sessionSequence.ProgressIndicator = runtimeWindow;

			splashScreen.Show();

			initialized = bootstrapSequence.TryPerform();

			if (initialized)
			{
				logger.Info("--- Application successfully initialized! ---");
				logger.Log(string.Empty);
				logger.Subscribe(runtimeWindow);

				splashScreen.Hide();

				StartSession(true);
			}
			else
			{
				logger.Info("--- Application startup aborted! ---");
				logger.Log(string.Empty);
			}

			return initialized;
		}

		public void Terminate()
		{
			// TODO: Necessary here? Move to App.cs as private "started" flag if not...
			if (!initialized)
			{
				return;
			}

			// TODO: Only if session is running!
			StopSession();

			logger.Unsubscribe(runtimeWindow);
			runtimeWindow?.Close();
			splashScreen?.Show();

			logger.Log(string.Empty);
			logger.Info("--- Initiating shutdown procedure ---");

			// TODO:
			// - Disconnect from service
			// - Terminate runtime communication host
			// - Revert kiosk mode (or do that when stopping session?)
			var success = bootstrapSequence.TryRevert();

			if (success)
			{
				logger.Info("--- Application successfully finalized! ---");
				logger.Log(string.Empty);
			}
			else
			{
				logger.Info("--- Shutdown procedure failed! ---");
				logger.Log(string.Empty);
			}

			splashScreen?.Close();
		}

		private void StartSession(bool initial = false)
		{
			logger.Info("Starting new session...");

			runtimeWindow.UpdateText(TextKey.RuntimeWindow_StartSession, true);
			runtimeWindow.Show();

			var success = initial ? sessionSequence.TryPerform() : sessionSequence.TryRepeat();

			if (success)
			{
				// TODO:
				// - Initialize session data
				// - Create and connect to client
				// - Initialize session with service
				// - Verify session integrity and start event handling



				runtimeWindow.HideProgressBar();
				runtimeWindow.UpdateText(TextKey.RuntimeWindow_ApplicationRunning);

				if (configuration.CurrentSettings.KioskMode == KioskMode.DisableExplorerShell)
				{
					runtimeWindow.Hide();
				}
			}
			else
			{
				uiFactory.Show(TextKey.MessageBox_SessionStartError, TextKey.MessageBox_SessionStartErrorTitle, icon: MessageBoxIcon.Error);

				if (initial)
				{
					initialized = false;
				}
				else
				{
					shutdown();
				}
			}
		}

		private void StopSession()
		{
			logger.Info("Stopping current session...");
			runtimeWindow.Show();
			runtimeWindow.BringToForeground();
			runtimeWindow.ShowProgressBar();
			runtimeWindow.UpdateText(TextKey.RuntimeWindow_StopSession, true);

			// TODO:
			// - Terminate client (or does it terminate itself?)
			// - Finalize session with service
			// - Stop event handling and close session
			var success = sessionSequence.TryRevert();

			if (success)
			{
				// TODO
			}
			else
			{
				// TODO
			}
		}
	}
}
