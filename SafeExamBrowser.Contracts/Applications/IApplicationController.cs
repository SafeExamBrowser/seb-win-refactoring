/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.UserInterface.Shell;

namespace SafeExamBrowser.Contracts.Applications
{
	/// <summary>
	/// Controls the lifetime and functionality of a (third-party) application which can be accessed via the <see cref="ITaskbar"/>.
	/// </summary>
	public interface IApplicationController
	{
		/// <summary>
		/// Performs any initialization work, if necessary.
		/// </summary>
		void Initialize();

		/// <summary>
		/// Registers an application control for this application.
		/// </summary>
		void RegisterApplicationControl(IApplicationControl control);

		/// <summary>
		/// Starts the execution of the application.
		/// </summary>
		void Start();

		/// <summary>
		/// Performs any termination work, e.g. releasing of used resources.
		/// </summary>
		void Terminate();
	}
}
