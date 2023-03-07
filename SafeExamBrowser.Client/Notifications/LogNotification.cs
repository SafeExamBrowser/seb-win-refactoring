/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Core.Contracts.Notifications;
using SafeExamBrowser.Core.Contracts.Notifications.Events;
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Client.Notifications
{
	internal class LogNotification : INotification
	{
		private readonly ILogger logger;
		private readonly IUserInterfaceFactory uiFactory;

		private IWindow window;

		public string Tooltip { get; }
		public IconResource IconResource { get; }

		public event NotificationChangedEventHandler NotificationChanged { add { } remove { } }

		public LogNotification(ILogger logger, IText text, IUserInterfaceFactory uiFactory)
		{
			this.logger = logger;
			this.uiFactory = uiFactory;

			IconResource = new BitmapIconResource { Uri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/LogNotification.ico") };
			Tooltip = text.Get(TextKey.Notification_LogTooltip);
		}

		public void Activate()
		{
			if (window == default)
			{
				window = uiFactory.CreateLogWindow(logger);

				window.Closed += () => window = default;
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
