/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Client;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.UserInterface.Shell;
using SafeExamBrowser.Contracts.UserInterface.Windows;

namespace SafeExamBrowser.Client.Notifications
{
	internal class AboutNotificationController : INotificationController
	{
		private INotificationControl notification;
		private AppConfig appConfig;
		private IUserInterfaceFactory uiFactory;
		private IWindow window;

		public AboutNotificationController(AppConfig appConfig, IUserInterfaceFactory uiFactory)
		{
			this.appConfig = appConfig;
			this.uiFactory = uiFactory;
		}

		public void RegisterNotification(INotificationControl notification)
		{
			this.notification = notification;

			notification.Clicked += Notification_Clicked;
		}

		public void Terminate()
		{
			window?.Close();
		}

		private void Notification_Clicked()
		{
			if (window == null)
			{
				window = uiFactory.CreateAboutWindow(appConfig);

				window.Closing += () => window = null;
				window.Show();
			}
			else
			{
				window.BringToForeground();
			}
		}
	}
}
