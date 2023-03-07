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
	/// Defines a request filter rule.
	/// </summary>
	public interface IRule
	{
		/// <summary>
		/// The filter result to be used if the rule matches a request.
		/// </summary>
		FilterResult Result { get; }

		/// <summary>
		/// Initializes the rule for processing requests.
		/// </summary>
		void Initialize(FilterRuleSettings settings);

		/// <summary>
		/// Indicates whether the rule applies for the given request.
		/// </summary>
		bool IsMatch(Request request);
	}
}
