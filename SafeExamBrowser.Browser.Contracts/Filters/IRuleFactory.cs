/*
 * Copyright (c) 2023 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Settings.Browser.Filter;

namespace SafeExamBrowser.Browser.Contracts.Filters
{
	/// <summary>
	/// Builds request filter rules.
	/// </summary>
	public interface IRuleFactory
	{
		/// <summary>
		/// Creates a filter rule for the given type.
		/// </summary>
		IRule CreateRule(FilterRuleType type);
	}
}
