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
		private ISplashScreen splashScreen;
		private IText text;

		public StartupController(ILogger logger, IMessageBox messageBox, ISplashScreen splashScreen, IText text)
		{
			this.logger = logger;
			this.messageBox = messageBox;
			this.splashScreen = splashScreen;
			this.text = text;
		}

		public void InitializeApplication(Action terminationCallback)
		{
			try
			{
				logger.Info("Rendering splash screen.");
				logger.Subscribe(splashScreen);
				splashScreen.Show();

				// TODO (depending on specification):
				// - WCF service connection, termination if not available

				// TODO:
				// - Parse command line arguments
				// - Detecting operating system and logging information
				// - Logging of all running processes
				// - Setting of wallpaper
				// - Initialization of taskbar
				// - Killing explorer.exer
				// - Minimizing all open windows
				// - Emptying clipboard
				// - Activation of process monitoring

				Thread.Sleep(3000);

				logger.Info("Baapa-dee boopa-dee!");

				Thread.Sleep(3000);

				logger.Info("Closing splash screen.");
				logger.Unsubscribe(splashScreen);
				splashScreen.Close();

				logger.Info("Application successfully initialized!");
			}
			catch (Exception e)
			{
				logger.Error($"Failed to initialize application!", e);
				messageBox.Show(text.Get(Key.MessageBox_StartupError), text.Get(Key.MessageBox_StartupErrorTitle), icon: MessageBoxIcon.Error);
				terminationCallback?.Invoke();
			}
		}
	}
}
