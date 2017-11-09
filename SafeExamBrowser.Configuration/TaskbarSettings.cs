/*
 * Copyright (c) 2017 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Contracts.Configuration.Settings;

namespace SafeExamBrowser.Configuration
{
	public class TaskbarSettings : ITaskbarSettings
	{
		public bool AllowApplicationLog => true;
		public bool AllowKeyboardLayout => true;
		public bool AllowWirelessNetwork => true;
	}
}
