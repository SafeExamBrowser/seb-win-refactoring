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
	/// A module which can be used to activate the <see cref="ITaskbar"/>.
	/// </summary>
	public interface ITaskbarActivator : IActivator
	{
		/// <summary>
		/// Fired when the taskbar should be activated (i.e. put into focus).
		/// </summary>
		event ActivatorEventHandler Activated;
	}
}
