/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.Contracts.UserInterface.Taskbar;

namespace SafeExamBrowser.Client.Notifications
{
	internal class AboutNotificationController : INotificationController
	{
		private INotificationButton notification;
		private IRuntimeInfo runtimeInfo;
		private IUserInterfaceFactory uiFactory;
		private IWindow window;

		public AboutNotificationController(IRuntimeInfo runtimeInfo, IUserInterfaceFactory uiFactory)
		{
			this.runtimeInfo = runtimeInfo;
			this.uiFactory = uiFactory;
		}

		public void RegisterNotification(INotificationButton notification)
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
				window = uiFactory.CreateAboutWindow(runtimeInfo);

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
