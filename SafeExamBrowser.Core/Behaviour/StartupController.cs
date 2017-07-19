/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.Monitoring;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Core.Behaviour
{
	public class StartupController : IStartupController
	{
		private IApplicationController browserController;
		private IApplicationInfo browserInfo;
		private ILogger logger;
		private IMessageBox messageBox;
		private INotificationInfo aboutInfo;
		private IProcessMonitor processMonitor;
		private ISettings settings;
		private ISplashScreen splashScreen;
		private ITaskbar taskbar;
		private IText text;
		private IUiElementFactory uiFactory;

		private IEnumerable<Action> StartupOperations
		{
			get
			{
				yield return HandleCommandLineArguments;
				yield return DetectOperatingSystem;
				yield return EstablishWcfServiceConnection;
				yield return DeactivateWindowsFeatures;
				yield return InitializeProcessMonitoring;
				yield return InitializeWorkArea;
				yield return InitializeTaskbar;
				yield return InitializeBrowser;
				yield return FinishInitialization;
			}
		}

		public StartupController(
			IApplicationController browserController,
			IApplicationInfo browserInfo,
			ILogger logger,
			IMessageBox messageBox,
			INotificationInfo aboutInfo,
			IProcessMonitor processMonitor,
			ISettings settings,
			ITaskbar taskbar,
			IText text,
			IUiElementFactory uiFactory)
		{
			this.browserController = browserController;
			this.browserInfo = browserInfo;
			this.logger = logger;
			this.messageBox = messageBox;
			this.aboutInfo = aboutInfo;
			this.processMonitor = processMonitor;
			this.settings = settings;
			this.taskbar = taskbar;
			this.text = text;
			this.uiFactory = uiFactory;
		}

		public bool TryInitializeApplication()
		{
			try
			{
				InitializeApplicationLog();
				InitializeSplashScreen();

				foreach (var operation in StartupOperations)
				{
					operation();
					splashScreen.UpdateProgress();

					// TODO: Remove!
					Thread.Sleep(250);
				}

				return true;
			}
			catch (Exception e)
			{
				logger.Error($"Failed to initialize application!", e);
				messageBox.Show(text.Get(Key.MessageBox_StartupError), text.Get(Key.MessageBox_StartupErrorTitle), icon: MessageBoxIcon.Error);

				return false;
			}
		}

		private void InitializeApplicationLog()
		{
			var titleLine = $"/* {settings.ProgramTitle}, Version {settings.ProgramVersion}{Environment.NewLine}";
			var copyrightLine = $"/* {settings.ProgramCopyright}{Environment.NewLine}";
			var emptyLine = $"/* {Environment.NewLine}";
			var githubLine = $"/* Please visit https://github.com/SafeExamBrowser for more information.";

			logger.Log($"{titleLine}{copyrightLine}{emptyLine}{githubLine}");
			logger.Log($"{Environment.NewLine}# Application started at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}{Environment.NewLine}");
			logger.Info("Initiating startup procedure.");
		}

		private void InitializeSplashScreen()
		{
			splashScreen = uiFactory.CreateSplashScreen(settings, text);
			splashScreen.SetMaxProgress(StartupOperations.Count());
			splashScreen.UpdateText(Key.SplashScreen_StartupProcedure);
			splashScreen.InvokeShow();
		}

		private void HandleCommandLineArguments()
		{
			// TODO
		}

		private void DetectOperatingSystem()
		{
			// TODO
		}

		private void EstablishWcfServiceConnection()
		{
			// TODO
		}

		private void DeactivateWindowsFeatures()
		{
			// TODO
		}

		private void InitializeProcessMonitoring()
		{
			logger.Info("Initializing process monitoring.");
			splashScreen.UpdateText(Key.SplashScreen_InitializeProcessMonitoring);

			// TODO

			processMonitor.StartMonitoringExplorer();
		}

		private void InitializeWorkArea()
		{
			logger.Info("Initializing work area.");
			splashScreen.UpdateText(Key.SplashScreen_InitializeWorkArea);

			// TODO
			// - Minimizing all open windows
			// - Emptying clipboard

			splashScreen.UpdateText(Key.SplashScreen_WaitExplorerTermination);
			splashScreen.StartBusyIndication();
			processMonitor.CloseExplorerShell();
			splashScreen.StopBusyIndication();
		}

		private void InitializeTaskbar()
		{
			logger.Info("Initializing taskbar.");
			splashScreen.UpdateText(Key.SplashScreen_InitializeTaskbar);

			// TODO

			var aboutNotification = uiFactory.CreateNotification(aboutInfo);

			taskbar.AddNotification(aboutNotification);
		}

		private void InitializeBrowser()
		{
			logger.Info("Initializing browser.");
			splashScreen.UpdateText(Key.SplashScreen_InitializeBrowser);

			// TODO

			var browserButton = uiFactory.CreateApplicationButton(browserInfo);

			browserController.RegisterApplicationButton(browserButton);
			taskbar.AddButton(browserButton);
		}

		private void FinishInitialization()
		{
			logger.Info("Application successfully initialized!");
			splashScreen.InvokeClose();
		}
	}
}
