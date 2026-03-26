/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;

namespace SafeExamBrowser.WindowsApi.Contracts
{
	/// <summary>
	/// Defines the style of a native window.
	/// </summary>
	public interface IWindowStyle
	{
		/// <summary>
		/// Indicates whether the window is disabled and cannot receive input from a user.
		/// </summary>
		bool IsDisabled { get; }

		/// <summary>
		/// Indicates whether the window does not get activated by the system nor when a user interacts with it.
		/// </summary>
		bool IsNotActivatable { get; }

		/// <summary>
		/// Indicates whether the window is above all other not topmost windows and remaining on top even when deactivated.
		/// </summary>
		bool IsTopmost { get; }

		/// <summary>
		/// Indicates whether the window is visible.
		/// </summary>
		bool IsVisible { get; }

		/// <summary>
		/// Returns the raw styles as queried from the operating system.
		/// </summary>
		IEnumerable<string> GetRawStyles();
	}
}
