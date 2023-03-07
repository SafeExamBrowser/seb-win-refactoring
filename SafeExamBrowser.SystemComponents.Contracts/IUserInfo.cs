/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.SystemComponents.Contracts
{
	/// <summary>
	/// Provides information about the currently logged in user.
	/// </summary>
	public interface IUserInfo
	{
		/// <summary>
		/// Retrieves the name of the currently logged in user.
		/// </summary>
		string GetUserName();

		/// <summary>
		/// Retrieves the security identifier of the currently logged in user.
		/// </summary>
		string GetUserSid();

		/// <summary>
		/// Tries to retrieve the security identifier for the specified user name. Returns <c>true</c> if successful, otherwise <c>false</c>.
		/// </summary>
		bool TryGetSidForUser(string userName, out string sid);
	}
}
