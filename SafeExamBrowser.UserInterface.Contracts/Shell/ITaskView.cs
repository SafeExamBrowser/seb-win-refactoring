/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Applications.Contracts;

namespace SafeExamBrowser.UserInterface.Contracts.Shell
{
	/// <summary>
	/// The task view provides an overview of all currently running application instances.
	/// </summary>
	public interface ITaskview
	{
		/// <summary>
		/// Adds the given application to the task view.
		/// </summary>
		void Add(IApplication application);

		/// <summary>
		/// Registers the specified activator for the task view.
		/// </summary>
		void Register(ITaskviewActivator activator);
	}
}
