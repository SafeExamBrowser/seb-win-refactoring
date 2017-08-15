/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.UserInterface.Taskbar
{
	public delegate void NotificationButtonClickedEventHandler();

	public interface INotificationButton
	{
		/// <summary>
		/// Event fired when the user clicked on the notification icon.
		/// </summary>
		event NotificationButtonClickedEventHandler Clicked;
	}
}
