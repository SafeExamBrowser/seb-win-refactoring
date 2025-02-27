/*
 * Copyright (c) 2025 ETH Zürich, IT Services
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

namespace SafeExamBrowser.UserInterface.Desktop.Controls.ActionCenter
{
	internal partial class NotificationButton : UserControl, INotificationControl
	{
		private readonly INotification notification;

		internal NotificationButton(INotification notification)
		{
			this.notification = notification;

			InitializeComponent();
			InitializeNotification();
			UpdateNotification();
		}

		private void IconButton_Click(object sender, RoutedEventArgs e)
		{
			if (notification.CanActivate)
			{
				notification.Activate();
			}
		}

		private void InitializeNotification()
		{
			notification.NotificationChanged += () => Dispatcher.Invoke(UpdateNotification);
		}

		private void UpdateNotification()
		{
			Icon.Content = IconResourceLoader.Load(notification.IconResource);
			IconButton.IsEnabled = notification.CanActivate;
			IconButton.ToolTip = notification.Tooltip;
			Text.Text = notification.Tooltip;
		}
	}
}
