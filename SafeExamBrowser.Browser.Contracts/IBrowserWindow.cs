/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Applications.Contracts;

namespace SafeExamBrowser.Browser.Contracts
{
	/// <summary>
	/// Defines a window of the <see cref="IBrowserApplication"/>.
	/// </summary>
	public interface IBrowserWindow : IApplicationWindow
	{
		/// <summary>
		/// Indicates whether the window is the main browser window.
		/// </summary>
		bool IsMainWindow { get; }

		/// <summary>
		/// The currently loaded URL, or <c>default(string)</c> in case no navigation has happened yet.
		/// </summary>
		string Url { get; }
	}
}
