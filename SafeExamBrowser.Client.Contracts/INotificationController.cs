/*
 * Copyright (c) 2020 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Client.Contracts
{
	/// <summary>
	/// Controls the lifetime and functionality of a notification which can be activated via the UI.
	/// </summary>
	public interface INotificationController
	{
		/// <summary>
		/// Executes the notification functionality.
		/// </summary>
		void Activate();
		
		/// <summary>
		/// Instructs the controller to shut down and release all used resources.
		/// </summary>
		void Terminate();
	}
}
