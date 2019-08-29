/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SafeExamBrowser.Contracts.Network.Filter;

namespace SafeExamBrowser.Contracts.Configuration.Settings
{
	/// <summary>
	/// Defines all configuration options for network functionality.
	/// </summary>
	[Serializable]
	public class NetworkSettings
	{
		/// <summary>
		/// Defines whether all content requests for a web page should be filtered according to the defined <see cref="FilterRules"/>.
		/// </summary>
		public bool EnableContentRequestFilter { get; set; }

		/// <summary>
		/// Defines whether the main request for a web page should be filtered according to the defined <see cref="FilterRules"/>.
		/// </summary>
		public bool EnableMainRequestFilter { get; set; }

		/// <summary>
		/// Defines all rules to be used to filter network requests.
		/// </summary>
		public IList<FilterRule> FilterRules { get; set; }

		public NetworkSettings()
		{
			FilterRules = new List<FilterRule>();
		}
	}
}
