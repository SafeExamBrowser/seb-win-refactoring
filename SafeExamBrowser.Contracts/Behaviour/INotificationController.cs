/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.UserInterface;

namespace SafeExamBrowser.Contracts.Behaviour
{
	public interface INotificationController
	{
		/// <summary>
		/// Registers the taskbar notification.
		/// </summary>
		void RegisterNotification(ITaskbarNotification notification);

		/// <summary>
		/// Instructs the controller to shut down and release all used resources.
		/// </summary>
		void Terminate();
	}
}
