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
		private bool sessionRunning;

		private IClientProxy client;
		private IConfigurationRepository configuration;
		private ILogger logger;
		private IOperationSequence bootstrapSequence;
		private IOperationSequence sessionSequence;
		private IRuntimeHost runtimeHost;
		private RuntimeInfo runtimeInfo;
		private IRuntimeWindow runtimeWindow;
		private IServiceProxy service;
		private ISplashScreen splashScreen;
		private Action shutdown;
		private IUserInterfaceFactory uiFactory;
		
		public RuntimeController(
			IClientProxy client,
			IConfigurationRepository configuration,
			ILogger logger,
			IOperationSequence bootstrapSequence,
			IOperationSequence sessionSequence,
			IRuntimeHost runtimeHost,
			RuntimeInfo runtimeInfo,
			IServiceProxy service,
			Action shutdown,
			IUserInterfaceFactory uiFactory)
		{
			this.client = client;
			this.configuration = configuration;
			this.logger = logger;
			this.bootstrapSequence = bootstrapSequence;
			this.sessionSequence = sessionSequence;
			this.runtimeHost = runtimeHost;
			this.runtimeInfo = runtimeInfo;
			this.service = service;
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

			var initialized = bootstrapSequence.TryPerform();

			if (initialized)
			{
				logger.Info("--- Application successfully initialized ---");
				logger.Log(string.Empty);
				logger.Subscribe(runtimeWindow);

				splashScreen.Hide();
				runtimeHost.ShutdownRequested += RuntimeHost_ShutdownRequested;

				StartSession(true);
			}
			else
			{
				logger.Info("--- Application startup aborted! ---");
				logger.Log(string.Empty);
			}

			return initialized && sessionRunning;
		}

		public void Terminate()
		{
			if (sessionRunning)
			{
				StopSession();
			}

			logger.Unsubscribe(runtimeWindow);
			runtimeWindow?.Close();
			splashScreen?.Show();
			splashScreen?.BringToForeground();

			logger.Log(string.Empty);
			logger.Info("--- Initiating shutdown procedure ---");

			var success = bootstrapSequence.TryRevert();

			if (success)
			{
				logger.Info("--- Application successfully finalized ---");
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
			runtimeWindow.Show();
			logger.Info(">------ Initiating session procedure ------<");

			sessionRunning = initial ? sessionSequence.TryPerform() : sessionSequence.TryRepeat();

			if (sessionRunning)
			{
				logger.Info(">------ Session is running ------<");
				runtimeWindow.HideProgressBar();
				runtimeWindow.UpdateText(TextKey.RuntimeWindow_ApplicationRunning);

				if (configuration.CurrentSettings.KioskMode == KioskMode.DisableExplorerShell)
				{
					runtimeWindow.Hide();
				}
			}
			else
			{
				logger.Info(">------ Session procedure was aborted! ------<");
				// TODO: Not when user chose to terminate after reconfiguration! Probably needs IOperationSequenceResult or alike...
				uiFactory.Show(TextKey.MessageBox_SessionStartError, TextKey.MessageBox_SessionStartErrorTitle, icon: MessageBoxIcon.Error);

				if (!initial)
				{
					shutdown.Invoke();
				}
			}
		}

		private void StopSession()
		{
			runtimeWindow.Show();
			runtimeWindow.BringToForeground();
			runtimeWindow.ShowProgressBar();
			logger.Info(">------ Reverting session operations ------<");

			var success = sessionSequence.TryRevert();

			if (success)
			{
				logger.Info(">------ Session is terminated ------<");
				sessionRunning = false;

				// TODO
			}
			else
			{
				logger.Info(">------ Session reversion was erroneous! ------<");

				// TODO
			}
		}

		private void RuntimeHost_ShutdownRequested()
		{
			logger.Info("Received shutdown request from client application.");
			shutdown.Invoke();
		}
	}
}
