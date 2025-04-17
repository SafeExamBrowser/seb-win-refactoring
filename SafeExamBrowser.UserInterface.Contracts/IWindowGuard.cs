/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.UserInterface.Contracts
{
	/// <summary>
	/// Provides functionality to guard application windows against capturing, recording and remote control.
	/// </summary>
	public interface IWindowGuard
	{
		/// <summary>
		/// Activates the guarding functionality for all previously and prospectively registered windows.
		/// </summary>
		void Activate();

		/// <summary>
		/// Deactivates the guarding functionality and clears the cache of previously registered windows.
		/// </summary>
		void Deactivate();

		/// <summary>
		/// Registers the given window for guarding (which depends on the window lifecycle and guard activation).
		/// </summary>
		/// <exception cref="System.ArgumentException">In case the given object is not an application window.</exception>
		void Register(object window);
	}
}
