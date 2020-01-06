/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Applications.Contracts.Resources.Icons;
using SafeExamBrowser.Client.Contracts;
using SafeExamBrowser.I18n.Contracts;

namespace SafeExamBrowser.Client.Notifications
{
	internal class AboutNotificationInfo : INotificationInfo
	{
		public string Tooltip { get; }
		public IconResource IconResource { get; }

		public AboutNotificationInfo(IText text)
		{
			IconResource =  new XamlIconResource { Uri = new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/AboutNotification.xaml") };
			Tooltip = text.Get(TextKey.Notification_AboutTooltip);
		}
	}
}
