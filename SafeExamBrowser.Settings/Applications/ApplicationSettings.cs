/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;

namespace SafeExamBrowser.Settings.Applications
{
	/// <summary>
	/// Defines all settings for external applications and application monitoring.
	/// </summary>
	[Serializable]
	public class ApplicationSettings
	{
		/// <summary>
		/// All applications which are not allowed to run during a session.
		/// </summary>
		public IList<BlacklistApplication> Blacklist { get; set; }

		/// <summary>
		/// All applications which are allowed to run during a session.
		/// </summary>
		public IList<WhitelistApplication> Whitelist { get; set; }

		public ApplicationSettings()
		{
			Blacklist = new List<BlacklistApplication>();
			Whitelist = new List<WhitelistApplication>();
		}
	}
}
