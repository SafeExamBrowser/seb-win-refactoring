/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.UserInterface.Taskbar;

namespace SafeExamBrowser.Contracts.SystemComponents
{
	public interface ISystemComponent<TControl> where TControl : ISystemControl
	{
		/// <summary>
		/// Initializes the resources used by the component and starts its operations, if applicable.
		/// </summary>
		void Initialize();

		/// <summary>
		/// Registers the taskbar control for the system component.
		/// </summary>
		void RegisterControl(TControl control);

		/// <summary>
		/// Instructs the component to stop any running operations and release all used resources.
		/// </summary>
		void Terminate();
	}
}
