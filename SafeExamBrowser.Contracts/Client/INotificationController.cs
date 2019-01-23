/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.UserInterface.Taskbar;

namespace SafeExamBrowser.Contracts.Client
{
	/// <summary>
	/// Controls the lifetime and functionality of a notification which is part of the <see cref="ITaskbar"/>.
	/// </summary>
	public interface INotificationController
	{
		/// <summary>
		/// Registers the taskbar notification.
		/// </summary>
		void RegisterNotification(INotificationButton notification);

		/// <summary>
		/// Instructs the controller to shut down and release all used resources.
		/// </summary>
		void Terminate();
	}
}
