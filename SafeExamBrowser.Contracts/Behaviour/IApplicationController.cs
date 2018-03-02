/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.UserInterface.Taskbar;

namespace SafeExamBrowser.Contracts.Behaviour
{
	/// <summary>
	/// Controls the lifetime and functionality of a (third-party) application which can be accessed via the taskbar.
	/// </summary>
	public interface IApplicationController
	{
		/// <summary>
		/// Performs any initialization work, if necessary.
		/// </summary>
		void Initialize();

		/// <summary>
		/// Registers the taskbar button for this application.
		/// </summary>
		void RegisterApplicationButton(IApplicationButton button);

		/// <summary>
		/// Performs any termination work, e.g. releasing of used resources.
		/// </summary>
		void Terminate();
	}
}
