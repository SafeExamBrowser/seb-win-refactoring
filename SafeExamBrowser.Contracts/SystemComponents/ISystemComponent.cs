/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.UserInterface.Shell;

namespace SafeExamBrowser.Contracts.SystemComponents
{
	/// <summary>
	/// Defines the functionality of a system component (e.g. the power supply). Each system component will get an <see cref="ISystemControl"/>
	/// assigned, via which the user is able to interact with or get information about the underlying system component.
	/// </summary>
	public interface ISystemComponent<TControl> where TControl : ISystemControl
	{
		/// <summary>
		/// Initializes the resources and operations of the component and registers its taskbar control.
		/// </summary>
		void Initialize(TControl control);

		/// <summary>
		/// Instructs the component to stop any running operations and releases all used resources.
		/// </summary>
		void Terminate();
	}
}
