/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.WindowsApi.Contracts
{
	/// <summary>
	/// Represents a desktop and defines its functionality.
	/// </summary>
	public interface IDesktop
	{
		/// <summary>
		/// The handle identifying the desktop.
		/// </summary>
		IntPtr Handle { get; }

		/// <summary>
		/// The name of the desktop.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Activates the desktop.
		/// </summary>
		/// <exception cref="System.ComponentModel.Win32Exception">If the desktop could not be activated.</exception>
		void Activate();

		/// <summary>
		/// Closes the desktop.
		/// </summary>
		/// <exception cref="System.ComponentModel.Win32Exception">If the desktop could not be closed.</exception>
		void Close();
	}
}
