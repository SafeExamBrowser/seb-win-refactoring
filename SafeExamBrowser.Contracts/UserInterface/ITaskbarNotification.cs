/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.UserInterface
{
	public delegate void TaskbarNotificationClickHandler();

	public interface ITaskbarNotification
	{
		/// <summary>
		/// OnClick handler, executed when the user clicks on the notification icon.
		/// </summary>
		event TaskbarNotificationClickHandler OnClick;
	}
}
