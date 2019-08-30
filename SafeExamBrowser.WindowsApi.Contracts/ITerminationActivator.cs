/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.WindowsApi.Contracts.Events;

namespace SafeExamBrowser.WindowsApi.Contracts
{
	/// <summary>
	/// A module which observes user input and indicates when the user would like to terminate the application.
	/// </summary>
	public interface ITerminationActivator
	{
		/// <summary>
		/// Fired when a termination request has been detected.
		/// </summary>
		event TerminationActivatorEventHandler Activated;

		/// <summary>
		/// Temporarily stops processing all user input.
		/// </summary>
		void Pause();

		/// <summary>
		/// Resumes processing user input.
		/// </summary>
		void Resume();

		/// <summary>
		/// Starts monitoring user input events.
		/// </summary>
		void Start();

		/// <summary>
		/// Stops monitoring user input events.
		/// </summary>
		void Stop();
	}
}
