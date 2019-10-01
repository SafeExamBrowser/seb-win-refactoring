/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
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
	/// TODO
	/// </summary>
	[Serializable]
	public class ApplicationSettings
	{
		/// <summary>
		/// 
		/// </summary>
		public IList<BlacklistApplication> Blacklist { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public IList<WhitelistApplication> Whitelist { get; set; }

		public ApplicationSettings()
		{
			Blacklist = new List<BlacklistApplication>();
			Whitelist = new List<WhitelistApplication>();
		}
	}
}
