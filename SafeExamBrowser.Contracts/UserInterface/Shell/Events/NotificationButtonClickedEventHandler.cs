/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.UserInterface.Shell.Events
{
	/// <summary>
	/// Indicates that the user clicked on a <see cref="INotificationButton"/> in the <see cref="ITaskbar"/>.
	/// </summary>
	public delegate void NotificationButtonClickedEventHandler();
}
