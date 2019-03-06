/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.UserInterface.Shell
{
	/// <summary>
	/// The action center is a user interface element via which the user can access and control various aspects of the application.
	/// </summary>
	public interface IActionCenter
	{
		/// <summary>
		/// Closes the action center.
		/// </summary>
		void Close();

		/// <summary>
		/// Makes the action center invisible.
		/// </summary>
		void Hide();

		/// <summary>
		/// Registers the specified activator to control the visibility of the action center.
		/// </summary>
		void Register(IActionCenterActivator activator);

		/// <summary>
		/// Makes the action center visible.
		/// </summary>
		void Show();
	}
}
