/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.UserInterface.Contracts.Windows.Data
{
	/// <summary>
	/// Defines the user interaction result of an <see cref="ICredentialsDialog"/>.
	/// </summary>
	public class CredentialsDialogResult
	{
		/// <summary>
		/// The password entered by the user, or <c>default(string)</c> if the interaction was unsuccessful.
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// Indicates whether the user confirmed the dialog or not.
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		/// The username entered by the user, or <c>default(string)</c> if no username is required or the interaction was unsuccessful.
		/// </summary>
		public string Username { get; set; }
	}
}
