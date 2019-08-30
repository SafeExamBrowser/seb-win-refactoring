/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SafeExamBrowser.Core.Contracts;

namespace SafeExamBrowser.Client.Notifications
{
	internal class AboutNotificationIconResource : IIconResource
	{
		public Uri Uri => new Uri("pack://application:,,,/SafeExamBrowser.UserInterface.Desktop;component/Images/AboutNotification.xaml");
		public bool IsBitmapResource => false;
		public bool IsXamlResource => true;
	}
}
