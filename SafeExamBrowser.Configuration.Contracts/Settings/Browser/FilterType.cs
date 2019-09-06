/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Configuration.Contracts.Settings.Browser
{
	/// <summary>
	/// Defines all possible request filter types.
	/// </summary>
	public enum FilterType
	{
		/// <summary>
		/// The filter is based on a regular expression.
		/// </summary>
		Regex,

		/// <summary>
		/// The filter is based on a simplified expression with wildcards.
		/// </summary>
		Simplified
	}
}
