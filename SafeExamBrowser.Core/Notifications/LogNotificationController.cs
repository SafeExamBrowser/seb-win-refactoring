/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Behaviour;
using SafeExamBrowser.Contracts.I18n;
using SafeExamBrowser.Contracts.Logging;
using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Core.Notifications
{
	public class LogNotificationController : INotificationController
	{
		private ITaskbarNotification notification;
		private ILogger logger;
		private ILogContentFormatter formatter;
		private IText text;
		private IUserInterfaceFactory uiFactory;
		private IWindow window;

		public LogNotificationController(ILogger logger, ILogContentFormatter formatter, IText text, IUserInterfaceFactory uiFactory)
		{
			this.logger = logger;
			this.formatter = formatter;
			this.text = text;
			this.uiFactory = uiFactory;
		}

		public void RegisterNotification(ITaskbarNotification notification)
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
				window = uiFactory.CreateLogWindow(logger, formatter, text);

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
