/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Monitoring.Contracts.Applications
{
	/// <summary>
	/// Represents a native window handled by the operating system.
	/// </summary>
	public interface IWindow
	{
		/// <summary>
		/// The handle of the window.
		/// </summary>
		IntPtr Handle { get; }

		/// <summary>
		/// The title of the window.
		/// </summary>
		string Title { get; }
	}
}
