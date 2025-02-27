/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.WindowsApi.Contracts;

namespace SafeExamBrowser.Monitoring.Contracts.Applications
{
	/// <summary>
	/// Provides information about the currently active application.
	/// </summary>
	public class ActiveApplication
	{
		/// <summary>
		/// The process which owns the currently active window.
		/// </summary>
		public IProcess Process { get; }

		/// <summary>
		/// The currently active window (i.e. the window which currently has input focus).
		/// </summary>
		public IWindow Window { get; }

		public ActiveApplication(IProcess process, IWindow window)
		{
			Process = process;
			Window = window;
		}
	}
}
