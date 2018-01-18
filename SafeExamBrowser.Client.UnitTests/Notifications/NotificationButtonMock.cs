/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.UserInterface.Taskbar;

namespace SafeExamBrowser.Client.UnitTests.Notifications
{
	class NotificationButtonMock : INotificationButton
	{
		private NotificationButtonClickedEventHandler clicked;

		public bool HasSubscribed;
		public bool HasUnsubscribed;

		public event NotificationButtonClickedEventHandler Clicked
		{
			add
			{
				clicked += value;
				HasSubscribed = true;
			}
			remove
			{
				clicked -= value;
				HasUnsubscribed = true;
			}
		}

		public void Click()
		{
			clicked?.Invoke();
		}
	}
}
