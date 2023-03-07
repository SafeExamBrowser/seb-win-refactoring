/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.UserInterface.Contracts.Shell
{
	/// <summary>
	/// Defines an activator for a shell component.
	/// </summary>
	public interface IActivator
	{
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
