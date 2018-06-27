/*
 * Copyright (c) 2018 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Contracts.UserInterface.Windows
{
	/// <summary>
	/// Defines the user interaction result of an <see cref="IPasswordDialog"/>.
	/// </summary>
	public interface IPasswordDialogResult
	{
		/// <summary>
		/// The password entered by the user, or <c>null</c> if the interaction was unsuccessful.
		/// </summary>
		string Password { get; }

		/// <summary>
		/// Indicates whether the user confirmed the dialog or not.
		/// </summary>
		bool Success { get; }
	}
}
