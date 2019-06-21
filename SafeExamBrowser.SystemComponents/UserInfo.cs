/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Security.Principal;
using SafeExamBrowser.Contracts.SystemComponents;

namespace SafeExamBrowser.SystemComponents
{
	public class UserInfo : IUserInfo
	{
		public string GetUserName()
		{
			return Environment.UserName;
		}

		public string GetUserSid()
		{
			return WindowsIdentity.GetCurrent().User.Value;
		}
	}
}
