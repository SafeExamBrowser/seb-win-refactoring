/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts.MessageBox;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Runtime.Responsibilities
{
	internal class ErrorMessageResponsibility : RuntimeResponsibility
	{
		private readonly AppConfig appConfig;
		private readonly IMessageBox messageBox;
		private readonly ISplashScreen splashScreen;
		private readonly IText text;

		internal ErrorMessageResponsibility(
			AppConfig appConfig,
			ILogger logger,
			IMessageBox messageBox,
			RuntimeContext runtimeContext,
			ISplashScreen splashScreen,
			IText text) : base(logger, runtimeContext)
		{
			this.appConfig = appConfig;
			this.messageBox = messageBox;
			this.splashScreen = splashScreen;
			this.text = text;
		}

		public override void Assume(RuntimeTask task)
		{
			switch (task)
			{
				case RuntimeTask.ShowShutdownError:
					ShowShutdownErrorMessage();
					break;
				case RuntimeTask.ShowStartupError:
					ShowStartupErrorMessage();
					break;
			}
		}

		private void ShowShutdownErrorMessage()
		{
			var message = AppendLogFilePaths(appConfig, text.Get(TextKey.MessageBox_ShutdownError));
			var title = text.Get(TextKey.MessageBox_ShutdownErrorTitle);

			messageBox.Show(message, title, icon: MessageBoxIcon.Error, parent: splashScreen);
		}

		private void ShowStartupErrorMessage()
		{
			var message = AppendLogFilePaths(appConfig, text.Get(TextKey.MessageBox_StartupError));
			var title = text.Get(TextKey.MessageBox_StartupErrorTitle);

			messageBox.Show(message, title, icon: MessageBoxIcon.Error, parent: splashScreen);
		}
	}
}
