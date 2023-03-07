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
	/// A module which observes user input and indicates when the user would like to terminate SEB.
	/// </summary>
	public interface ITerminationActivator : IActivator
	{
		/// <summary>
		/// Fired when a termination request has been detected.
		/// </summary>
		event ActivatorEventHandler Activated;
	}
}
