/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace SafeExamBrowser.Contracts.Network.Filter
{
	/// <summary>
	/// Defines a network filter rule.
	/// </summary>
	[Serializable]
	public class FilterRule
	{
		/// <summary>
		/// The expression according to which requests should be filtered.
		/// </summary>
		public string Expression { get; set; }

		/// <summary>
		/// The filter result to be used when the <see cref="Expression"/> matches.
		/// </summary>
		public FilterResult Result { get; set; }

		/// <summary>
		/// The filter type which defines how the <see cref="Expression"/> is processed.
		/// </summary>
		public FilterType Type { get; set; }
	}
}
