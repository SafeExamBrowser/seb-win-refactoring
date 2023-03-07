/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.UserInterface.Contracts.Windows.Data;

namespace SafeExamBrowser.UserInterface.Contracts.Windows
{
	/// <summary>
	/// Defines the functionality of a lock screen which covers all active displays and prevents the user from continuing their work.
	/// </summary>
	public interface ILockScreen : IWindow
	{
		/// <summary>
		/// Cancels the <see cref="WaitForResult"/> operation and closes the lock screen.
		/// </summary>
		void Cancel();

		/// <summary>
		/// Expands the lock screen across all active displays and resizes it accordingly.
		/// </summary>
		void InitializeBounds();

		/// <summary>
		/// Waits for the user to provide the required input to unlock the application.
		/// </summary>
		LockScreenResult WaitForResult();
	}
}
