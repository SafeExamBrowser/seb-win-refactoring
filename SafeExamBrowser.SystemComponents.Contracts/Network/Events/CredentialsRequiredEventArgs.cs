/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.SystemComponents.Contracts.Network.Events
{
	/// <summary>
	/// The event arguments for the <see cref="CredentialsRequiredEventHandler"/>.
	/// </summary>
	public class CredentialsRequiredEventArgs
	{
		/// <summary>
		/// The name of the network which requires credentials.
		/// </summary>
		public string NetworkName { get; set; }

		/// <summary>
		/// The password as specified by the user.
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// Indicates whether the credentials could be successfully retrieved or not.
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		/// The username as specified by the user.
		/// </summary>
		public string Username { get; set; }
	}
}
