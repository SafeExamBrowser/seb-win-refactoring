/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Settings.Applications
{
	/// <summary>
	/// Defines an application which is whitelisted, i.e. allowed to run during a session.
	/// </summary>
	[Serializable]
	public class WhitelistApplication
	{
		/// <summary>
		/// The name of the main executable of the application.
		/// </summary>
		public string ExecutableName { get; set; }
	}
}
