/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.WindowsApi.Contracts
{
	/// <summary>
	/// The factory for desktops of the operating system.
	/// </summary>
	public interface IDesktopFactory
	{
		/// <summary>
		/// Creates a new desktop with the given name.
		/// </summary>
		/// <exception cref="System.ComponentModel.Win32Exception">If the desktop could not be created.</exception>
		IDesktop CreateNew(string name);

		/// <summary>
		/// Retrieves the currently active desktop.
		/// </summary>
		/// <exception cref="System.ComponentModel.Win32Exception">If the current desktop could not be retrieved.</exception>
		IDesktop GetCurrent();
	}
}
