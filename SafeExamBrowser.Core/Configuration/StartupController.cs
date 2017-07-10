/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Threading;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Core.Configuration
{
	public class StartupController : IStartupController
	{
		private ILogger logger;
		private IMessageBox messageBox;
		private ISettings settings;
		private ISplashScreen splashScreen;
		private ITaskbar taskbar;
		private IText text;

		public StartupController(ILogger logger, IMessageBox messageBox, ISettings settings, ISplashScreen splashScreen, ITaskbar taskbar, IText text)
		{
			this.logger = logger;
			this.messageBox = messageBox;
			this.settings = settings;
			this.splashScreen = splashScreen;
			this.taskbar = taskbar;
			this.text = text;
		}

		public bool TryInitializeApplication()
		{
			try
			{
				logger.Log(settings.LogHeader);
				logger.Log($"{Environment.NewLine}# Application started at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}{Environment.NewLine}");
				logger.Info("Initiating startup procedure.");

				logger.Subscribe(splashScreen);

				splashScreen.SetMaxProgress(4);
				splashScreen.UpdateProgress();

				// TODO (depending on specification):
				// - WCF service connection, termination if not available

				// TODO:
				// - Parse command line arguments
				// - Detecting operating system and log that information
				// - Logging of all running processes
				// - Setting of wallpaper
				// - Initialization of taskbar
				// - Killing explorer.exer
				// - Minimizing all open windows
				// - Emptying clipboard
				// - Activation of process monitoring

				Thread.Sleep(3000);

				splashScreen.UpdateProgress();
				logger.Info("Baapa-dee boopa-dee!");

				Thread.Sleep(3000);

				splashScreen.UpdateProgress();
				logger.Info("Closing splash screen.");

				Thread.Sleep(3000);

				splashScreen.UpdateProgress();
				logger.Unsubscribe(splashScreen);
				logger.Info("Application successfully initialized!");

				return true;
			}
			catch (Exception e)
			{
				logger.Error($"Failed to initialize application!", e);
				messageBox.Show(text.Get(Key.MessageBox_StartupError), text.Get(Key.MessageBox_StartupErrorTitle), icon: MessageBoxIcon.Error);

				return false;
			}
		}
	}
}
