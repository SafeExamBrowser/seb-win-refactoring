/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Behaviour.OperationModel;
using SafeExamBrowser.Contracts.Communication.Events;
using SafeExamBrowser.Contracts.Communication.Hosts;
using SafeExamBrowser.Contracts.Communication.Proxies;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.Configuration.Settings;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.UserInterface.MessageBox;
using SafeExamBrowser.Contracts.UserInterface.Windows;

namespace SafeExamBrowser.Runtime.Behaviour
{
	internal class RuntimeController : IRuntimeController
	{
		private bool sessionRunning;

		private AppConfig appConfig;
		private IConfigurationRepository configuration;
		private ILogger logger;
		private IMessageBox messageBox;
		private IOperationSequence bootstrapSequence;
		private IOperationSequence sessionSequence;
		private IRuntimeHost runtimeHost;
		private IRuntimeWindow runtimeWindow;
		private IServiceProxy service;
		private ISplashScreen splashScreen;
		private Action shutdown;
		private IUserInterfaceFactory uiFactory;
		
		public RuntimeController(
			AppConfig appConfig,
			IConfigurationRepository configuration,
			ILogger logger,
			IMessageBox messageBox,
			IOperationSequence bootstrapSequence,
			IOperationSequence sessionSequence,
			IRuntimeHost runtimeHost,
			IServiceProxy service,
			Action shutdown,
			IUserInterfaceFactory uiFactory)
		{
			this.appConfig = appConfig;
			this.configuration = configuration;
			this.bootstrapSequence = bootstrapSequence;
			this.logger = logger;
			this.messageBox = messageBox;
			this.runtimeHost = runtimeHost;
			this.sessionSequence = sessionSequence;
			this.service = service;
			this.shutdown = shutdown;
			this.uiFactory = uiFactory;
		}

		public bool TryStart()
		{
			logger.Info("Initiating startup procedure...");

			runtimeWindow = uiFactory.CreateRuntimeWindow(appConfig);
			splashScreen = uiFactory.CreateSplashScreen(appConfig);

			bootstrapSequence.ProgressIndicator = splashScreen;
			sessionSequence.ProgressIndicator = runtimeWindow;

			splashScreen.Show();

			var initialized = bootstrapSequence.TryPerform() == OperationResult.Success;

			if (initialized)
			{
				RegisterEvents();

				logger.Info("Application successfully initialized.");
				logger.Log(string.Empty);
				logger.Subscribe(runtimeWindow);
				splashScreen.Hide();

				StartSession(true);
			}
			else
			{
				logger.Info("Application startup aborted!");
				logger.Log(string.Empty);

				messageBox.Show(TextKey.MessageBox_StartupError, TextKey.MessageBox_StartupErrorTitle, icon: MessageBoxIcon.Error);
			}

			return initialized && sessionRunning;
		}

		public void Terminate()
		{
			DeregisterEvents();

			if (sessionRunning)
			{
				StopSession();
			}

			logger.Unsubscribe(runtimeWindow);
			runtimeWindow?.Close();
			splashScreen?.Show();
			splashScreen?.BringToForeground();

			logger.Log(string.Empty);
			logger.Info("Initiating shutdown procedure...");

			var success = bootstrapSequence.TryRevert();

			if (success)
			{
				logger.Info("Application successfully finalized.");
				logger.Log(string.Empty);
			}
			else
			{
				logger.Info("Shutdown procedure failed!");
				logger.Log(string.Empty);

				messageBox.Show(TextKey.MessageBox_ShutdownError, TextKey.MessageBox_ShutdownErrorTitle, icon: MessageBoxIcon.Error);
			}

			splashScreen?.Close();
		}

		private void StartSession(bool initial = false)
		{
			runtimeWindow.Show();
			runtimeWindow.BringToForeground();
			runtimeWindow.ShowProgressBar();
			logger.Info("Initiating session procedure...");

			if (sessionRunning)
			{
				DeregisterSessionEvents();
			}

			var result = initial ? sessionSequence.TryPerform() : sessionSequence.TryRepeat();

			if (result == OperationResult.Success)
			{
				RegisterSessionEvents();

				logger.Info("Session is running.");
				runtimeWindow.HideProgressBar();
				runtimeWindow.UpdateText(TextKey.RuntimeWindow_ApplicationRunning);
				runtimeWindow.TopMost = configuration.CurrentSettings.KioskMode != KioskMode.None;

				if (configuration.CurrentSettings.KioskMode == KioskMode.DisableExplorerShell)
				{
					runtimeWindow.Hide();
				}

				sessionRunning = true;
			}
			else
			{
				logger.Info($"Session procedure {(result == OperationResult.Aborted ? "was aborted." : "has failed!")}");

				if (result == OperationResult.Failed)
				{
					// TODO: Check if message box is rendered on new desktop as well! -> E.g. if settings for reconfiguration are invalid
					messageBox.Show(TextKey.MessageBox_SessionStartError, TextKey.MessageBox_SessionStartErrorTitle, icon: MessageBoxIcon.Error);

					if (!initial)
					{
						logger.Info("Terminating application...");
						shutdown.Invoke();
					}
				}
			}
		}

		private void StopSession()
		{
			runtimeWindow.Show();
			runtimeWindow.BringToForeground();
			runtimeWindow.ShowProgressBar();
			logger.Info("Reverting session operations...");

			DeregisterSessionEvents();

			var success = sessionSequence.TryRevert();

			if (success)
			{
				logger.Info("Session is terminated.");
				sessionRunning = false;
			}
			else
			{
				logger.Info("Session reversion was erroneous!");
				messageBox.Show(TextKey.MessageBox_SessionStopError, TextKey.MessageBox_SessionStopErrorTitle, icon: MessageBoxIcon.Error);
			}
		}

		private void RegisterEvents()
		{
			runtimeHost.ReconfigurationRequested += RuntimeHost_ReconfigurationRequested;
			runtimeHost.ShutdownRequested += RuntimeHost_ShutdownRequested;
		}

		private void DeregisterEvents()
		{
			runtimeHost.ReconfigurationRequested -= RuntimeHost_ReconfigurationRequested;
			runtimeHost.ShutdownRequested -= RuntimeHost_ShutdownRequested;
		}

		private void RegisterSessionEvents()
		{
			configuration.CurrentSession.ClientProcess.Terminated += ClientProcess_Terminated;
			configuration.CurrentSession.ClientProxy.ConnectionLost += Client_ConnectionLost;
		}

		private void DeregisterSessionEvents()
		{
			configuration.CurrentSession.ClientProcess.Terminated -= ClientProcess_Terminated;
			configuration.CurrentSession.ClientProxy.ConnectionLost -= Client_ConnectionLost;
		}

		private void ClientProcess_Terminated(int exitCode)
		{
			logger.Error($"Client application has unexpectedly terminated with exit code {exitCode}!");
			// TODO: Check if message box is rendered on new desktop as well -> otherwise shutdown is blocked!
			messageBox.Show(TextKey.MessageBox_ApplicationError, TextKey.MessageBox_ApplicationErrorTitle, icon: MessageBoxIcon.Error);

			shutdown.Invoke();
		}

		private void Client_ConnectionLost()
		{
			logger.Error("Lost connection to the client application!");
			// TODO: Check if message box is rendered on new desktop as well -> otherwise shutdown is blocked!
			messageBox.Show(TextKey.MessageBox_ApplicationError, TextKey.MessageBox_ApplicationErrorTitle, icon: MessageBoxIcon.Error);

			shutdown.Invoke();
		}

		private void RuntimeHost_ReconfigurationRequested(ReconfigurationEventArgs args)
		{
			var mode = configuration.CurrentSettings.ConfigurationMode;

			if (mode == ConfigurationMode.ConfigureClient)
			{
				logger.Info($"Accepted request for reconfiguration with '{args.ConfigurationPath}'.");
				configuration.ReconfigurationFilePath = args.ConfigurationPath;

				StartSession();
			}
			else
			{
				logger.Info($"Denied request for reconfiguration with '{args.ConfigurationPath}' due to '{mode}' mode!");
				configuration.CurrentSession.ClientProxy.InformReconfigurationDenied(args.ConfigurationPath);
			}
		}

		private void RuntimeHost_ShutdownRequested()
		{
			logger.Info("Received shutdown request from the client application.");
			shutdown.Invoke();
		}
	}
}
