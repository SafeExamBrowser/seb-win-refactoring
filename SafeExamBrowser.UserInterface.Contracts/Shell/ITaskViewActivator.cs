/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.UserInterface.Contracts.Shell.Events;

namespace SafeExamBrowser.UserInterface.Contracts.Shell
{
	/// <summary>
	/// A module which can be used to control the <see cref="ITaskView"/>.
	/// </summary>
	public interface ITaskViewActivator : IActivator
	{
		/// <summary>
		/// Fired when the next application instance should be selected.
		/// </summary>
		event ActivatorEventHandler Next;

		/// <summary>
		/// Fired when the previous application instance should be selected.
		/// </summary>
		event ActivatorEventHandler Previous;
	}
}
