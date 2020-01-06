/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Client.Contracts;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Client.Notifications
{
	internal class AboutNotificationController : INotificationController
	{
		private AppConfig appConfig;
		private IUserInterfaceFactory uiFactory;
		private IWindow window;

		public AboutNotificationController(AppConfig appConfig, IUserInterfaceFactory uiFactory)
		{
			this.appConfig = appConfig;
			this.uiFactory = uiFactory;
		}

		public void Activate()
		{
			if (window == default(IWindow))
			{
				window = uiFactory.CreateAboutWindow(appConfig);

				window.Closing += () => window = default(IWindow);
				window.Show();
			}
			else
			{
				window.BringToForeground();
			}
		}

		public void Terminate()
		{
			window?.Close();
		}
	}
}
