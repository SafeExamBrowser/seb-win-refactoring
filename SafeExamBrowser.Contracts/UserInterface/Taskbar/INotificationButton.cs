/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.UserInterface.Taskbar
{
	public delegate void NotificationButtonClickedEventHandler();

	/// <summary>
	/// The button of a notification which can be loaded into the <see cref="ITaskbar"/>.
	/// </summary>
	public interface INotificationButton
	{
		/// <summary>
		/// Event fired when the user clicked on the notification icon.
		/// </summary>
		event NotificationButtonClickedEventHandler Clicked;
	}
}
