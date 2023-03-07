/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.Notifications;
using SafeExamBrowser.Core.Contracts.Notifications.Events;
using SafeExamBrowser.Core.Contracts.Resources.Icons;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Windows;

namespace SafeExamBrowser.Client.Notifications
{
	internal class AboutNotification : INotification
	{
		private readonly AppConfig appConfig;
		private readonly IUserInterfaceFactory uiFactory;

		private IWindow window;

		public string Tooltip { get; }
		public IconResource IconResource { get; }

		public event NotificationChangedEventHandler NotificationChanged { add { } remove { } }

		public AboutNotification(AppConfig appConfig, IText text, IUserInterfaceFactory uiFactory)
		{
			this.appConfig = appConfig;
			this.uiFactory = uiFactory;

			IconResource = new XamlIconResource { Uri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/AboutNotification.xaml") };
			Tooltip = text.Get(TextKey.Notification_AboutTooltip);
		}

		public void Activate()
		{
			if (window == default)
			{
				window = uiFactory.CreateAboutWindow(appConfig);

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
