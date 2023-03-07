/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Settings.Browser.Filter;

namespace SafeExamBrowser.Settings.Browser
{
	/// <summary>
	/// Defines all settings for the request filter of the browser.
	/// </summary>
	[Serializable]
	public class FilterSettings
	{
		/// <summary>
		/// Defines whether content requests for a web page should be filtered according to the defined <see cref="Rules"/>.
		/// </summary>
		public bool ProcessContentRequests { get; set; }

		/// <summary>
		/// Defines whether the main request for a web page should be filtered according to the defined <see cref="Rules"/>.
		/// </summary>
		public bool ProcessMainRequests { get; set; }

		/// <summary>
		/// Defines all rules to be used to filter web requests.
		/// </summary>
		public IList<FilterRuleSettings> Rules { get; set; }

		public FilterSettings()
		{
			Rules = new List<FilterRuleSettings>();
		}
	}
}
