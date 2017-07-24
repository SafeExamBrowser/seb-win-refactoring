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
	public interface IApplicationController
	{
		/// <summary>
		/// Performs any initialization work, if necessary.
		/// </summary>
		void Initialize();

		/// <summary>
		/// Registers the taskbar button for this application.
		/// </summary>
		void RegisterApplicationButton(ITaskbarButton button);

		/// <summary>
		/// Performs any termination work, e.g. freeing of resources.
		/// </summary>
		void Terminate();
	}
}
