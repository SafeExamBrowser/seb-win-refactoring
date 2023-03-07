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
	/// A module which can be used to control the <see cref="ITaskview"/>.
	/// </summary>
	public interface ITaskviewActivator : IActivator
	{
		/// <summary>
		/// Fired when the task view should be hidden.
		/// </summary>
		event ActivatorEventHandler Deactivated;

		/// <summary>
		/// Fired when the task view should be made visible and the next application instance should be selected.
		/// </summary>
		event ActivatorEventHandler NextActivated;

		/// <summary>
		/// Fired when the task view should be made visible and the previous application instance should be selected.
		/// </summary>
		event ActivatorEventHandler PreviousActivated;
	}
}
