/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Settings.Browser.Filter
{
	/// <summary>
	/// Defines all possible filter rule types.
	/// </summary>
	public enum FilterRuleType
	{
		/// <summary>
		/// The filter rule is based on a regular expression.
		/// </summary>
		Regex,

		/// <summary>
		/// The filter rule is based on a simplified expression with wildcards.
		/// </summary>
		Simplified
	}
}
