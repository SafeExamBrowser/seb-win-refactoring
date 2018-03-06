/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.WindowsApi
{
	public delegate void ProcessTerminatedEventHandler(int exitCode);

	/// <summary>
	/// Represents a process and defines its functionality.
	/// </summary>
	public interface IProcess
	{
		/// <summary>
		/// Indicates whether the process has been terminated.
		/// </summary>
		bool HasTerminated { get; }

		/// <summary>
		/// The process identifier.
		/// </summary>
		int Id { get; }

		/// <summary>
		/// Event fired when the process has terminated.
		/// </summary>
		event ProcessTerminatedEventHandler Terminated;

		/// <summary>
		/// Immediately terminates the process.
		/// </summary>
		void Kill();
	}
}
