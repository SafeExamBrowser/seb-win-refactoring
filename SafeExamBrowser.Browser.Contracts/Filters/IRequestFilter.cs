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
	/// Defines the filter for browser requests.
	/// </summary>
	public interface IRequestFilter
	{
		/// <summary>
		/// The default result to be returned by <see cref="Process(Request)"/> if no rule matches.
		/// </summary>
		FilterResult Default { get; set; }

		/// <summary>
		/// Loads the given filter rule to be used when processing requests.
		/// </summary>
		void Load(IRule rule);

		/// <summary>
		/// Filters the given request according to the loaded rules.
		/// </summary>
		FilterResult Process(Request request);
	}
}
