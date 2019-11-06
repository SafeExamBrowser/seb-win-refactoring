/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Applications.Contracts.Events;

namespace SafeExamBrowser.Applications.Contracts
{
	/// <summary>
	/// Controls the lifetime and functionality of an application.
	/// </summary>
	public interface IApplication
	{
		/// <summary>
		/// Provides information about the application.
		/// </summary>
		ApplicationInfo Info { get; }

		/// <summary>
		/// Fired when a new <see cref="IApplicationInstance"/> has started.
		/// </summary>
		event InstanceStartedEventHandler InstanceStarted;

		/// <summary>
		/// Performs any initialization work, if necessary.
		/// </summary>
		void Initialize();

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
