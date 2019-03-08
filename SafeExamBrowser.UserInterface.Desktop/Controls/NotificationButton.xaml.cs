/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Controls;
using SafeExamBrowser.Contracts.Client;
using SafeExamBrowser.Contracts.UserInterface.Shell;
using SafeExamBrowser.Contracts.UserInterface.Shell.Events;
using SafeExamBrowser.UserInterface.Desktop.Utilities;

namespace SafeExamBrowser.UserInterface.Desktop.Controls
{
	public partial class NotificationButton : UserControl, INotificationControl
	{
		public event NotificationControlClickedEventHandler Clicked;

		public NotificationButton(INotificationInfo info)
		{
			InitializeComponent();
			InitializeNotificationIcon(info);
		}

		private void Icon_Click(object sender, RoutedEventArgs e)
		{
			Clicked?.Invoke();
		}

		private void InitializeNotificationIcon(INotificationInfo info)
		{
			IconButton.ToolTip = info.Tooltip;
			IconButton.Content = IconResourceLoader.Load(info.IconResource);
		}
	}
}
