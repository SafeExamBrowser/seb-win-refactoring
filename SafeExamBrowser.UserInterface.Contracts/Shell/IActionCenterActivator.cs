/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.UserInterface.Contracts.Shell.Events;

namespace SafeExamBrowser.UserInterface.Contracts.Shell
{
	/// <summary>
	/// A module which can be used to control the <see cref="IActionCenter"/>.
	/// </summary>
	public interface IActionCenterActivator : IActivator
	{
		/// <summary>
		/// Fired when the action center should be made visible.
		/// </summary>
		event ActivatorEventHandler Activated;

		/// <summary>
		/// Fired when the action center should be made invisible.
		/// </summary>
		event ActivatorEventHandler Deactivated;

		/// <summary>
		/// Fired when the action center visibility should be toggled.
		/// </summary>
		event ActivatorEventHandler Toggled;
	}
}
