/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows.Controls;
using SafeExamBrowser.Contracts.Configuration;
using SafeExamBrowser.Contracts.UserInterface;
using SafeExamBrowser.UserInterface.Utilities;

namespace SafeExamBrowser.UserInterface.Controls
{
	public partial class NotificationIcon : UserControl, ITaskbarNotification
	{
		public event TaskbarNotificationClickHandler OnClick;

		public NotificationIcon(INotificationInfo info)
		{
			InitializeComponent();
			InitializeNotificationIcon(info);
		}

		private void Icon_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			OnClick?.Invoke();
		}

		private void InitializeNotificationIcon(INotificationInfo info)
		{
			IconButton.ToolTip = info.Tooltip;
			IconButton.Content = IconResourceLoader.Load(info.IconResource);
		}
	}
}
