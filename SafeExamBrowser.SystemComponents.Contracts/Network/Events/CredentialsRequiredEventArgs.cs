/*
 * Copyright (c) 2024 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.SystemComponents.Contracts.Network.Events
{
	/// <summary>
	/// 
	/// </summary>
	public class CredentialsRequiredEventArgs
	{
		/// <summary>
		/// 
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string Username { get; set; }
	}
}
