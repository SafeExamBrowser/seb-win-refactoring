/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.UserInterface.Shell.Events;

namespace SafeExamBrowser.Contracts.UserInterface.Shell
{
	/// <summary>
	/// A module which can be used to control the visibility of the <see cref="IActionCenter"/>.
	/// </summary>
	public interface IActionCenterActivator
	{
		/// <summary>
		/// Fired when the action center should be made visible.
		/// </summary>
		event ActivatorEventHandler Activate;

		/// <summary>
		/// Fired when the action center should be made invisible.
		/// </summary>
		event ActivatorEventHandler Deactivate;

		/// <summary>
		/// Fired when the action center visibility should be toggled.
		/// </summary>
		event ActivatorEventHandler Toggle;

		/// <summary>
		/// Starts monitoring user input events.
		/// </summary>
		void Start();

		/// <summary>
		/// Stops monitoring user input events.
		/// </summary>
		void Stop();
	}
}
