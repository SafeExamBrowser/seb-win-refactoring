/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.SystemComponents.Contracts
{
	/// <summary>
	/// Defines the functionality of a system component (e.g. the power supply).
	/// </summary>
	public interface ISystemComponent
	{
		/// <summary>
		/// Initializes the resources and operations of the component.
		/// </summary>
		void Initialize();

		/// <summary>
		/// Instructs the component to stop any running operations and releases all used resources.
		/// </summary>
		void Terminate();
	}
}
