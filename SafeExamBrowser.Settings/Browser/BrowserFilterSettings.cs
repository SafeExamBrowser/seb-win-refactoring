/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;

namespace SafeExamBrowser.Settings.Browser
{
	/// <summary>
	/// Defines all configuration options for the request filter of the browser.
	/// </summary>
	[Serializable]
	public class BrowserFilterSettings
	{
		/// <summary>
		/// Defines whether all content requests for a web page should be filtered according to the defined <see cref="Rules"/>.
		/// </summary>
		public bool FilterContentRequests { get; set; }

		/// <summary>
		/// Defines whether the main request for a web page should be filtered according to the defined <see cref="Rules"/>.
		/// </summary>
		public bool FilterMainRequests { get; set; }

		/// <summary>
		/// Defines all rules to be used to filter web requests.
		/// </summary>
		public IList<FilterRuleSettings> Rules { get; set; }

		public BrowserFilterSettings()
		{
			Rules = new List<FilterRuleSettings>();
		}
	}
}
