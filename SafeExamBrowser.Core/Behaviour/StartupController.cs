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
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Core.Behaviour
{
	public class StartupController : IStartupController
	{
		private IApplicationInfo browserInfo;
		private ILogger logger;
		private IMessageBox messageBox;
		private ISettings settings;
		private ISplashScreen splashScreen;
		private ITaskbar taskbar;
		private IText text;
		private IUiElementFactory uiFactory;

		private IEnumerable<Action> StartupOperations
		{
			get
			{
				yield return InitializeApplicationLog;
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

		public StartupController(IApplicationInfo browserInfo, ILogger logger, IMessageBox messageBox, ISettings settings, ISplashScreen splashScreen, ITaskbar taskbar, IText text, IUiElementFactory uiFactory)
		{
			this.browserInfo = browserInfo;
			this.logger = logger;
			this.messageBox = messageBox;
			this.settings = settings;
			this.splashScreen = splashScreen;
			this.taskbar = taskbar;
			this.text = text;
			this.uiFactory = uiFactory;
		}

		public bool TryInitializeApplication()
		{
			try
			{
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
			logger.Log(settings.LogHeader);
			logger.Log($"{Environment.NewLine}# Application started at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}{Environment.NewLine}");
			logger.Info("Initiating startup procedure.");
			logger.Subscribe(splashScreen);

			splashScreen.SetMaxProgress(StartupOperations.Count());
		}

		private void HandleCommandLineArguments()
		{
			logger.Info("Parsing command line arguments.");

			// TODO
		}

		private void DetectOperatingSystem()
		{
			logger.Info("Detecting operating system.");

			// TODO
		}

		private void EstablishWcfServiceConnection()
		{
			logger.Info("Establishing connection to WCF service.");

			// TODO
		}

		private void DeactivateWindowsFeatures()
		{
			logger.Info("Deactivating Windows Update.");

			// TODO

			logger.Info("Disabling lock screen options.");

			// TODO
		}

		private void InitializeProcessMonitoring()
		{
			logger.Info("Initializing process monitoring.");

			// TODO
		}

		private void InitializeWorkArea()
		{
			logger.Info("Initializing work area.");

			// TODO
			// - Killing explorer.exe
			// - Minimizing all open windows
			// - Emptying clipboard
		}

		private void InitializeTaskbar()
		{
			logger.Info("Initializing taskbar.");

			// TODO
		}

		private void InitializeBrowser()
		{
			logger.Info("Initializing browser.");

			var browserButton = uiFactory.CreateButton(browserInfo);

			// TODO

			taskbar.AddButton(browserButton);
		}

		private void FinishInitialization()
		{
			logger.Info("Application successfully initialized!");
			logger.Unsubscribe(splashScreen);
		}
	}
}
