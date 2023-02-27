﻿/*
 * Copyright (c) 2022 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Controls;
using SafeExamBrowser.Core.Contracts.Notifications;
using SafeExamBrowser.UserInterface.Contracts.Shell;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace SafeExamBrowser.UserInterface.Desktop.Controls.Taskbar
{
	internal partial class NotificationButton : UserControl, INotificationControl
	{
		private INotification notification;

		internal NotificationButton(INotification notification)
		{
			this.notification = notification;

			InitializeComponent();
			InitializeNotification();
			UpdateNotification();
		}

		private void IconButton_Click(object sender, RoutedEventArgs e)
		{
			notification.Activate();
		}

		private void InitializeNotification()
		{
			notification.NotificationChanged += () => Dispatcher.Invoke(UpdateNotification);
		}

		private void UpdateNotification()
		{
			IconButton.ToolTip = notification.Tooltip;
			IconButton.Content = IconResourceLoader.Load(notification.IconResource);
			System.Windows.Automation.AutomationProperties.SetName(this, notification.Tooltip);
		}
	}
}
