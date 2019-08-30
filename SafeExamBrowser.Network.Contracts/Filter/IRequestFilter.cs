/*
 * Copyright (c) 2019 ETH Zürich, Educational Development and Technology (LET)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Network.Contracts.Filter
{
	/// <summary>
	/// Defines a filter to process network requests.
	/// </summary>
	public interface IRequestFilter
	{
		/// <summary>
		/// Defines the default result to be returned by <see cref="Process(Request)"/> if no filter rule matches.
		/// </summary>
		FilterResult Default { set; }

		/// <summary>
		/// Loads the given <see cref="FilterRule"/> to be used when processing a request.
		/// </summary>
		void Load(FilterRule rule);

		/// <summary>
		/// Filters the given request according to the loaded <see cref="FilterRule"/>.
		/// </summary>
		FilterResult Process(Request request);
	}
}
